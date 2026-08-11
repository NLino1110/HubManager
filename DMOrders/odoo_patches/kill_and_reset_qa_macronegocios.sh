#!/usr/bin/env bash
export PGPASSWORD=openpgpwd

echo "Matando restores duplicados..."
pkill -f 'bunzip2 -c .*macronegocios' 2>/dev/null || true
# no matar este script; matar restores viejos por nombre de work dir antiguo
ps -ef | awk '/restore_qa_macronegocios.sh/ && !/awk/ {print $2}' | while read -r pid; do
  if [[ "$pid" != "$$" && "$pid" != "$PPID" ]]; then
    kill "$pid" 2>/dev/null || true
  fi
done
sleep 2

psql -h localhost -U openpg -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='qa.macronegocios' AND pid <> pg_backend_pid();" || true

echo "Procesos restantes:"
ps -ef | grep -E 'macronegocios.sql|bunzip2 -c' | grep -v grep || echo "limpio"

echo "Drop/Create limpio"
psql -h localhost -U openpg -d postgres -c "DROP DATABASE IF EXISTS \"qa.macronegocios\";"
psql -h localhost -U openpg -d postgres -c "CREATE DATABASE \"qa.macronegocios\" WITH OWNER=openpg ENCODING='UTF8' TEMPLATE=template0;"
echo "BD vacia lista"
