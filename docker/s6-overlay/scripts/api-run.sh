#!/bin/sh
until pg_isready -h localhost -p 5432 -U "${POSTGRES_USER:-postgres}" >/dev/null 2>&1; do
    echo "[api] waiting for postgres..."
    sleep 1
done
cd /app/api
exec s6-setuidgid appuser dotnet TaskManager.API.dll
