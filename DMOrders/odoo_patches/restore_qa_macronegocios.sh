#!/usr/bin/env bash
set -euo pipefail
export PGPASSWORD=openpgpwd

DB_NAME="qa.macronegocios"
PGUSER_NAME="openpg"
SQL_BZ2="/mnt/d/macronegocios.sql.bz2"
FILESTORE_TGZ="/mnt/d/macronegocios_filestore.tar.gz"
DATA_DIR="$HOME/odoo/sessions"
WORK="$HOME/odoo/backups/restore_macronegocios_$(date +%Y%m%d_%H%M%S)"
mkdir -p "$WORK" "$DATA_DIR/filestore"

echo "==> Fuentes"
ls -lh "$SQL_BZ2" "$FILESTORE_TGZ"
echo "WORK=$WORK"

echo "==> Drop/Create $DB_NAME (omitir si ya se reseteó afuera)"
# Se asume BD ya creada por kill_and_reset o se recrea aquí
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='${DB_NAME}' AND pid <> pg_backend_pid();" || true
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "DROP DATABASE IF EXISTS \"${DB_NAME}\";"
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "CREATE DATABASE \"${DB_NAME}\" WITH OWNER=${PGUSER_NAME} ENCODING='UTF8' TEMPLATE=template0;"

echo "==> Copiando dump a disco Linux (reutiliza copia previa si existe)"
set +e
CACHED=$(ls -1t "$HOME"/odoo/backups/restore_macronegocios_*/macronegocios.sql.bz2 2>/dev/null | head -1)
set -e
if [[ -n "${CACHED:-}" && -f "$CACHED" ]]; then
  echo "Reutilizando $CACHED"
  LOCAL_BZ2="$CACHED"
else
  LOCAL_BZ2="$WORK/macronegocios.sql.bz2"
  cp -f "$SQL_BZ2" "$LOCAL_BZ2"
fi
ls -lh "$LOCAL_BZ2"

echo "==> Restore SQL (puede tardar 10-40+ min)"
set +eu
bunzip2 -c "$LOCAL_BZ2" | psql -h localhost -U "$PGUSER_NAME" -d "$DB_NAME" -v ON_ERROR_STOP=0 >"$WORK/restore.log" 2>&1
echo "done pipe" | tee -a "$WORK/restore.log"
set -eu
tail -40 "$WORK/restore.log" || true

echo "==> Extrayendo filestore"
mkdir -p "$WORK/filestore_extract"
tar -xzf "$FILESTORE_TGZ" -C "$WORK/filestore_extract"
ls -la "$WORK/filestore_extract" | head -20 || true

FS_SRC="$WORK/filestore_extract"
if [[ -d "$WORK/filestore_extract/filestore" ]]; then
  FS_SRC="$WORK/filestore_extract/filestore"
fi

echo "FS_SRC=$FS_SRC"
ls -la "$FS_SRC" | head -20 || true

echo "==> Instalando filestore en $DATA_DIR/filestore/$DB_NAME"
rm -rf "$DATA_DIR/filestore/$DB_NAME"
mkdir -p "$DATA_DIR/filestore/$DB_NAME"

if [[ -d "$FS_SRC/$DB_NAME" ]]; then
  cp -a "$FS_SRC/$DB_NAME"/. "$DATA_DIR/filestore/$DB_NAME/"
elif [[ -d "$FS_SRC/macronegocios" ]]; then
  cp -a "$FS_SRC/macronegocios"/. "$DATA_DIR/filestore/$DB_NAME/"
elif [[ -d "$FS_SRC/filestore/$DB_NAME" ]]; then
  cp -a "$FS_SRC/filestore/$DB_NAME"/. "$DATA_DIR/filestore/$DB_NAME/"
elif [[ -d "$FS_SRC/filestore/macronegocios" ]]; then
  cp -a "$FS_SRC/filestore/macronegocios"/. "$DATA_DIR/filestore/$DB_NAME/"
else
  child_count=$(find "$FS_SRC" -mindepth 1 -maxdepth 1 -type d | wc -l)
  if [[ "$child_count" -eq 1 ]]; then
    only=$(find "$FS_SRC" -mindepth 1 -maxdepth 1 -type d | head -1)
    cp -a "$only"/. "$DATA_DIR/filestore/$DB_NAME/"
  else
    cp -a "$FS_SRC"/. "$DATA_DIR/filestore/$DB_NAME/"
  fi
fi

du -sh "$DATA_DIR/filestore/$DB_NAME" | tee "$WORK/filestore_size.txt"
ls "$DATA_DIR/filestore/$DB_NAME" | head -20 || true

echo "==> Verify"
psql -h localhost -U "$PGUSER_NAME" -d "$DB_NAME" -tAc "SELECT count(*) FROM information_schema.tables WHERE table_schema='public';" | tee "$WORK/tables.txt"
psql -h localhost -U "$PGUSER_NAME" -d postgres -tAc "SELECT pg_size_pretty(pg_database_size('${DB_NAME}'));" | tee "$WORK/dbsize.txt"

echo "LISTO -> BD=$DB_NAME filestore=$DATA_DIR/filestore/$DB_NAME"
echo "WORK=$WORK"
