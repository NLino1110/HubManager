#!/usr/bin/env bash
set -euo pipefail
export PGPASSWORD=openpgpwd

DB_NAME="qa.demujeres"
DATA_DIR="$HOME/odoo/sessions"
SRC=$(ls -d "$HOME"/odoo/backups/restore_demujeres_*/filestore_extract/dmujeressa 2>/dev/null | tail -1)

echo "SRC=$SRC"
echo "DEST=$DATA_DIR/filestore/$DB_NAME"

if [[ -z "$SRC" || ! -d "$SRC" ]]; then
  echo "No hay extract de filestore; extrayendo tar..."
  WORK="$HOME/odoo/backups/restore_demujeres_fs_$(date +%Y%m%d_%H%M%S)"
  mkdir -p "$WORK"
  tar -xzf /mnt/d/dmujeressa_filestore.tar.gz -C "$WORK"
  SRC="$WORK/dmujeressa"
fi

mkdir -p "$DATA_DIR/filestore"
rm -rf "$DATA_DIR/filestore/$DB_NAME"
mkdir -p "$DATA_DIR/filestore/$DB_NAME"
cp -a "$SRC"/. "$DATA_DIR/filestore/$DB_NAME/"

echo "Filestore size:"
du -sh "$DATA_DIR/filestore/$DB_NAME"
echo "Top entries:"
ls "$DATA_DIR/filestore/$DB_NAME" | head -20

echo "DB tables:"
psql -h localhost -U openpg -d "$DB_NAME" -tAc "SELECT count(*) FROM information_schema.tables WHERE table_schema='public';"
echo "DB size:"
psql -h localhost -U openpg -d postgres -tAc "SELECT pg_size_pretty(pg_database_size('${DB_NAME}'));"

echo "LISTO: BD=$DB_NAME filestore=$DATA_DIR/filestore/$DB_NAME"
echo "Recuerda: dbfilter actual=qa.macronegocios -> cambia a False o qa.* y reinicia Odoo"
