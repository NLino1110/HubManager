#!/usr/bin/env bash
set -euo pipefail
export PGPASSWORD=openpgpwd

DB_NAME="qa.demujeres"
PGUSER_NAME="openpg"
SQL_BZ2="/mnt/d/dmujeressa.sql.bz2"
WORK="$HOME/odoo/backups/restore_demujeres_sql_$(date +%Y%m%d_%H%M%S)"
mkdir -p "$WORK"

echo "==> Drop/Create $DB_NAME"
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='${DB_NAME}' AND pid <> pg_backend_pid();" || true
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "DROP DATABASE IF EXISTS \"${DB_NAME}\";"
psql -h localhost -U "$PGUSER_NAME" -d postgres -c "CREATE DATABASE \"${DB_NAME}\" WITH OWNER=${PGUSER_NAME} ENCODING='UTF8' TEMPLATE=template0;"

echo "==> Copiando dump a disco Linux"
cp -f "$SQL_BZ2" "$WORK/dmujeressa.sql.bz2"
ls -lh "$WORK/dmujeressa.sql.bz2"

echo "==> Restore SQL (puede tardar 10-30+ min)"
set +eu
bunzip2 -c "$WORK/dmujeressa.sql.bz2" | psql -h localhost -U "$PGUSER_NAME" -d "$DB_NAME" -v ON_ERROR_STOP=0 >"$WORK/restore.log" 2>&1
echo "done pipe" | tee -a "$WORK/restore.log"
set -eu

echo "==> Tail log"
tail -50 "$WORK/restore.log" || true

echo "==> Verify"
psql -h localhost -U "$PGUSER_NAME" -d "$DB_NAME" -tAc "SELECT count(*) FROM information_schema.tables WHERE table_schema='public';" | tee "$WORK/tables.txt"
psql -h localhost -U "$PGUSER_NAME" -d postgres -tAc "SELECT pg_size_pretty(pg_database_size('${DB_NAME}'));" | tee "$WORK/dbsize.txt"

echo "LISTO SQL -> $DB_NAME"
echo "WORK=$WORK"
