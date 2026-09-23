#!/usr/bin/env bash
set -euo pipefail
export PGPASSWORD=openpgpwd

DB_NAME="qa.macronegocios"
PGUSER_NAME="openpg"
SQL_BZ2="/mnt/c/Users/Rchonillo/Documents/macronegocios.sql.bz2"
DATA_DIR="$HOME/odoo/sessions"
WORK="$HOME/odoo/backups/restore_macronegocios_$(date +%Y%m%d_%H%M%S)"
mkdir -p "$WORK" "$DATA_DIR/filestore"

echo "==> Fuentes"
ls -lh "$SQL_BZ2"
echo "WORK=$WORK"

echo "==> Estado actual"
psql -h localhost -U "$PGUSER_NAME" -d postgres -tAc "SELECT pg_size_pretty(pg_database_size('${DB_NAME}'));" 2>/dev/null || echo "BD no existe aun"
du -sh "$DATA_DIR/filestore/$DB_NAME" 2>/dev/null || echo "filestore no existe aun"

echo "==> Drop/Create $DB_NAME"
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='${DB_NAME}' AND pid <> pg_backend_pid();" || true
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "DROP DATABASE IF EXISTS \"${DB_NAME}\";"
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "CREATE DATABASE \"${DB_NAME}\" WITH OWNER=${PGUSER_NAME} ENCODING='UTF8' TEMPLATE=template0;"

echo "==> Copiando dump a disco Linux (acelera el restore)"
LOCAL_BZ2="$WORK/macronegocios.sql.bz2"
cp -f "$SQL_BZ2" "$LOCAL_BZ2"
ls -lh "$LOCAL_BZ2"

echo "==> Restore SQL (puede tardar 10-40+ min)"
set +eu
bunzip2 -c "$LOCAL_BZ2" | psql -h localhost -U "$PGUSER_NAME" -d "$DB_NAME" -v ON_ERROR_STOP=0 >"$WORK/restore.log" 2>&1
echo "done pipe" | tee -a "$WORK/restore.log"
set -eu
tail -40 "$WORK/restore.log" || true

echo "==> Verify"
psql -h localhost -U "$PGUSER_NAME" -d "$DB_NAME" -tAc "SELECT count(*) FROM information_schema.tables WHERE table_schema='public';" | tee "$WORK/tables.txt"
psql -h localhost -U "$PGUSER_NAME" -d postgres -tAc "SELECT pg_size_pretty(pg_database_size('${DB_NAME}'));" | tee "$WORK/dbsize.txt"

echo "LISTO -> BD=$DB_NAME restaurada desde $SQL_BZ2"
echo "WORK=$WORK"
echo "NOTA: filestore no incluido en este backup; el existente en $DATA_DIR/filestore/$DB_NAME se conserva."
