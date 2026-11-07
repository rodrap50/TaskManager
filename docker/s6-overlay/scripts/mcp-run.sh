#!/bin/sh
# Waits for Postgres like api-run.sh: TaskManager.Mcp migrates McpTracking on startup.
# With an external Postgres (DISABLE_BUNDLED_POSTGRES) there is nothing on localhost to wait for.
if [ -z "$DISABLE_BUNDLED_POSTGRES" ]; then
    until pg_isready -h localhost -p 5432 -U "${POSTGRES_USER:-postgres}" >/dev/null 2>&1; do
        echo "[mcp] waiting for postgres..."
        sleep 1
    done
fi
cd /app/mcp
# The container-wide ASPNETCORE_URLS is the API's (8080) — MCP listens on its own port.
export ASPNETCORE_URLS=http://+:8083
exec s6-setuidgid appuser dotnet TaskManager.Mcp.dll
