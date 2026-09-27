#!/bin/sh
# Listens on all container interfaces so other compose services (TaskManager.Mcp's
# McpTracking database) can reach it — pg_hba.conf limits that to `samenet`, and no
# host port is ever published for 5432 (see postgres-init.sh / docker-compose.yml).
exec s6-setuidgid appuser postgres -D "${PGDATA:-/var/lib/postgresql/data}" -h '*' -c unix_socket_directories=/tmp
