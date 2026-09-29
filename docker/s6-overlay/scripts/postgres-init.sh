#!/bin/sh
set -e

# s6-rc's oneshot runner invokes this as root — drop to the unprivileged app
# user before touching PGDATA or Postgres.
if [ "$(id -u)" = "0" ]; then
    exec s6-setuidgid appuser "$0" "$@"
fi

PGDATA="${PGDATA:-/var/lib/postgresql/data}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_DB="${POSTGRES_DB:-taskmanager}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-taskmanager_internal}"

if [ ! -s "$PGDATA/PG_VERSION" ]; then
    echo "[postgres-init] Initializing new Postgres data directory at $PGDATA"
    mkdir -p "$PGDATA"

    echo "$POSTGRES_PASSWORD" > /tmp/pg_pwfile
    initdb -D "$PGDATA" -U "$POSTGRES_USER" --pwfile=/tmp/pg_pwfile \
        --auth-local=trust --auth-host=scram-sha-256
    rm -f /tmp/pg_pwfile

    pg_ctl -D "$PGDATA" -o "-c listen_addresses='' -c unix_socket_directories=/tmp" -w start
    createdb -h /tmp -U "$POSTGRES_USER" "$POSTGRES_DB"
    pg_ctl -D "$PGDATA" -m fast -w stop
else
    echo "[postgres-init] Existing Postgres data directory found at $PGDATA, skipping initialization"
fi
