#!/bin/sh
if [ -n "$DISABLE_BUNDLED_POSTGRES" ]; then
    echo "[postgres] DISABLE_BUNDLED_POSTGRES is set — not starting the bundled Postgres"
    exec s6-pause
fi
exec s6-setuidgid appuser postgres -D "${PGDATA:-/var/lib/postgresql/data}" -h localhost -c unix_socket_directories=/tmp
