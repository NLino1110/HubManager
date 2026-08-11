#!/usr/bin/env bash
export PGPASSWORD=openpgpwd
echo "=== procesos ==="
ps -ef | grep -E 'bunzip2|psql -h localhost -U openpg -d qa.macronegocios|restore_qa_macro' | grep -v grep || echo none
echo "=== log lines ==="
LOG=$(ls -1t /home/rchonillo/odoo/backups/restore_macronegocios_*/restore.log 2>/dev/null | head -1)
echo "LOG=$LOG"
wc -l "$LOG" 2>/dev/null || true
tail -8 "$LOG" 2>/dev/null || true
echo "=== db size ==="
psql -h localhost -U openpg -d postgres -tAc "SELECT pg_size_pretty(pg_database_size('qa.macronegocios'));" 2>/dev/null || true
