#!/usr/bin/env bash
set -euo pipefail

SQL_BZ2="/mnt/d/dmujeressa.sql.bz2"
FILESTORE_TGZ="/mnt/d/dmujeressa_filestore.tar.gz"
DB_NAME="qa.demujeres"
DATA_DIR="$HOME/odoo/sessions"
WORK="$HOME/odoo/backups/restore_demujeres_$(date +%Y%m%d_%H%M%S)"
PGUSER_NAME="openpg"
export PGPASSWORD="openpgpwd"

echo "==> Fuentes"
ls -lh "$SQL_BZ2" "$FILESTORE_TGZ"

mkdir -p "$WORK" "$DATA_DIR/filestore"
echo "WORK=$WORK"
echo "DATA_DIR=$DATA_DIR"

echo "==> Listando filestore (primeras entradas)"
set +e
tar -tzf "$FILESTORE_TGZ" | head -30 | tee "$WORK/filestore_listing.txt"
set -e

echo "==> Extrayendo filestore a $WORK/filestore_extract"
mkdir -p "$WORK/filestore_extract"
tar -xzf "$FILESTORE_TGZ" -C "$WORK/filestore_extract"
ls -la "$WORK/filestore_extract" | head -40 | tee "$WORK/filestore_top.txt" || true

# Ubicar origen del filestore
FS_SRC="$WORK/filestore_extract"
if [[ -d "$WORK/filestore_extract/filestore" ]]; then
  FS_SRC="$WORK/filestore_extract/filestore"
fi

echo "FS_SRC=$FS_SRC"
ls -la "$FS_SRC" | head -30

echo "==> PostgreSQL: comprobar usuario/conexión"
psql -h localhost -U "$PGUSER_NAME" -d postgres -tAc "SELECT current_user;"
psql -h localhost -U "$PGUSER_NAME" -d postgres -tAc "SELECT datname FROM pg_database ORDER BY 1;" | tee "$WORK/databases_before.txt"

echo "==> Drop/Create database $DB_NAME"
psql -h localhost -U "$PGUSER_NAME" -d postgres -v ON_ERROR_STOP=1 -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '${DB_NAME}' AND pid <> pg_backend_pid();" || true
psql -h localhost -U "$PGUSER_NAME" -d postgres -v ON_ERROR_STOP=1 -c "DROP DATABASE IF EXISTS \"${DB_NAME}\";"
psql -h localhost -U "$PGUSER_NAME" -d postgres -v ON_ERROR_STOP=1 -c "CREATE DATABASE \"${DB_NAME}\" WITH OWNER = ${PGUSER_NAME} ENCODING = 'UTF8' TEMPLATE = template0;"

echo "==> Restaurando SQL (stream bunzip2 | psql) — puede tardar mucho"
# Copiar a disco Linux acelera mucho vs /mnt/d
LOCAL_BZ2="$WORK/dmujeressa.sql.bz2"
if [[ ! -f "$LOCAL_BZ2" ]]; then
  echo "Copiando dump a $LOCAL_BZ2 ..."
  cp -f "$SQL_BZ2" "$LOCAL_BZ2"
fi
set +eu
bunzip2 -c "$LOCAL_BZ2" | psql -h localhost -U "$PGUSER_NAME" -d "$DB_NAME" -v ON_ERROR_STOP=0 >"$WORK/restore.log" 2>&1
RC0=${PIPESTATUS[0]:-0}
RC1=${PIPESTATUS[1]:-0}
set -eu
echo "bunzip2_rc=$RC0 psql_rc=$RC1" | tee -a "$WORK/restore.log"
tail -40 "$WORK/restore.log" || true

echo "==> Instalando filestore en $DATA_DIR/filestore/$DB_NAME"
rm -rf "$DATA_DIR/filestore/$DB_NAME"
mkdir -p "$DATA_DIR/filestore/$DB_NAME"

if [[ -d "$FS_SRC/$DB_NAME" ]]; then
  cp -a "$FS_SRC/$DB_NAME"/. "$DATA_DIR/filestore/$DB_NAME/"
elif [[ -d "$FS_SRC/dmujeressa" ]]; then
  cp -a "$FS_SRC/dmujeressa"/. "$DATA_DIR/filestore/$DB_NAME/"
elif [[ -d "$FS_SRC/filestore/$DB_NAME" ]]; then
  cp -a "$FS_SRC/filestore/$DB_NAME"/. "$DATA_DIR/filestore/$DB_NAME/"
elif [[ -d "$FS_SRC/filestore/dmujeressa" ]]; then
  cp -a "$FS_SRC/filestore/dmujeressa"/. "$DATA_DIR/filestore/$DB_NAME/"
else
  # Si hay una sola carpeta hija, usarla; si hay hashes (ab, cd...), copiar todo
  child_count=$(find "$FS_SRC" -mindepth 1 -maxdepth 1 -type d | wc -l)
  if [[ "$child_count" -eq 1 ]]; then
    only=$(find "$FS_SRC" -mindepth 1 -maxdepth 1 -type d | head -1)
    cp -a "$only"/. "$DATA_DIR/filestore/$DB_NAME/"
  else
    cp -a "$FS_SRC"/. "$DATA_DIR/filestore/$DB_NAME/"
  fi
fi

du -sh "$DATA_DIR/filestore/$DB_NAME" | tee "$WORK/filestore_size.txt"
ls "$DATA_DIR/filestore/$DB_NAME" | head -20 | tee "$WORK/filestore_dest_top.txt"

echo "==> Verificación BD"
psql -h localhost -U "$PGUSER_NAME" -d "$DB_NAME" -tAc "SELECT count(*) AS tables FROM information_schema.tables WHERE table_schema='public';" | tee "$WORK/table_count.txt"

cat <<EOF

========================================
LISTO
BD:           $DB_NAME
Filestore:    $DATA_DIR/filestore/$DB_NAME
Logs/temp:    $WORK

NOTA: en odoo_linux.conf tienes dbfilter = qa.macronegocios
Para ver qa.demujeres cambia temporalmente a:
  dbfilter = False
o:
  dbfilter = qa.*
y reinicia Odoo.
========================================
EOF
