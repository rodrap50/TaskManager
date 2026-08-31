#!/bin/sh
exec s6-setuidgid appuser postgres -D "${PGDATA:-/var/lib/postgresql/data}" -h localhost -c unix_socket_directories=/tmp
