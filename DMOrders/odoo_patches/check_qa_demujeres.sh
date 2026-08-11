#!/usr/bin/env bash
set -euo pipefail
export PGPASSWORD=openpgpwd
echo "tables in qa.demujeres:"
psql -h localhost -U openpg -d qa.demujeres -tAc "SELECT count(*) FROM information_schema.tables WHERE table_schema='public';" || echo "db missing/error"
echo "filestore extract exists?"
ls -d /home/rchonillo/odoo/backups/restore_demujeres_*/filestore_extract/dmujeressa 2>/dev/null | tail -3 || true
