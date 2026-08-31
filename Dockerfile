# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Stage: build-api — publish the .NET API (Release)
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build-api
WORKDIR /src

COPY backend/TaskManager.slnx ./
COPY backend/TaskManager.API/TaskManager.API.csproj TaskManager.API/
COPY backend/TaskManager.Application/TaskManager.Application.csproj TaskManager.Application/
COPY backend/TaskManager.Domain/TaskManager.Domain.csproj TaskManager.Domain/
COPY backend/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj TaskManager.Infrastructure/
RUN dotnet restore "TaskManager.API/TaskManager.API.csproj"

COPY backend/ .
WORKDIR /src/TaskManager.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Stage: build-web — build the frontend (production, relative API paths)
# ---------------------------------------------------------------------------
FROM node:22-alpine AS build-web
WORKDIR /src

COPY frontend/package.json frontend/package-lock.json ./
COPY frontend/packages/shared/package.json packages/shared/
COPY frontend/packages/ui/package.json packages/ui/
COPY frontend/packages/web/package.json packages/web/
COPY frontend/packages/mobile/package.json packages/mobile/
RUN npm ci

COPY frontend/ .
# Empty VITE_API_URL => axios/resolveAssetUrl resolve every request relative
# to the page's own origin, since Nginx proxies /api and /avatars to the
# local API (see docker/nginx.conf) — no production domain needs to be known
# at build time. NOTE: must stay "" (empty), not "/api" — ApiClient.ts's
# endpoint paths already embed "/api/..." themselves.
ENV VITE_API_URL=
RUN npm run build --workspace=packages/web

# ---------------------------------------------------------------------------
# Stage: final — Postgres + API + Nginx, supervised by s6-overlay
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

ARG S6_OVERLAY_VERSION=3.2.0.2
ADD https://github.com/just-containers/s6-overlay/releases/download/v${S6_OVERLAY_VERSION}/s6-overlay-noarch.tar.xz /tmp/s6-overlay-noarch.tar.xz
ADD https://github.com/just-containers/s6-overlay/releases/download/v${S6_OVERLAY_VERSION}/s6-overlay-x86_64.tar.xz /tmp/s6-overlay-x86_64.tar.xz
RUN tar -C / -Jxpf /tmp/s6-overlay-noarch.tar.xz \
 && tar -C / -Jxpf /tmp/s6-overlay-x86_64.tar.xz \
 && rm -f /tmp/s6-overlay-noarch.tar.xz /tmp/s6-overlay-x86_64.tar.xz

RUN apk add --no-cache postgresql16 nginx \
 && rm -rf /var/cache/apk/*

RUN addgroup -S -g 1000 appuser \
 && adduser -S -G appuser -u 1000 -h /app appuser

# Postgres — bundled in this same container, not exposed externally.
ENV PGDATA=/var/lib/postgresql/data
ENV POSTGRES_USER=postgres
ENV POSTGRES_DB=taskmanager
ENV POSTGRES_PASSWORD=taskmanager_internal

# API — the default connection string below points at the bundled local
# Postgres above. Deliberately NOT the literal PREP.4 dev-fallback string
# (Program.cs's DevFallbackConnectionString), so the Production startup
# guard doesn't misfire — Postgres genuinely IS on localhost in this image.
# Override both this and POSTGRES_PASSWORD together via `docker run -e` if
# pointing at a different/external Postgres instead.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DatabaseSettings__ConnectionString="Host=localhost;Port=5432;Database=taskmanager;Username=postgres;Password=taskmanager_internal"
# Jwt__Secret intentionally has NO default here — required at `docker run`,
# per PREP.4's fail-fast Production guard.

COPY --from=build-api /app/publish /app/api
COPY --from=build-web /src/packages/web/dist /app/web
COPY docker/nginx.conf /etc/nginx/nginx.conf
COPY docker/s6-overlay/s6-rc.d /etc/s6-overlay/s6-rc.d
COPY docker/s6-overlay/scripts /etc/s6-overlay/scripts

RUN chmod +x /etc/s6-overlay/s6-rc.d/postgres-init/up \
             /etc/s6-overlay/s6-rc.d/postgres/run \
             /etc/s6-overlay/s6-rc.d/api/run \
             /etc/s6-overlay/s6-rc.d/nginx/run \
             /etc/s6-overlay/scripts/postgres-init.sh \
             /etc/s6-overlay/scripts/postgres-run.sh \
             /etc/s6-overlay/scripts/api-run.sh \
             /etc/s6-overlay/scripts/nginx-run.sh \
 && mkdir -p "$PGDATA" /app/api/wwwroot/avatars /var/lib/nginx /var/log/nginx \
 && chown -R appuser:appuser /app /var/lib/postgresql /var/lib/nginx /var/log/nginx /etc/nginx

# Postgres data (D02.2) and avatar uploads (A02) both need to survive
# container recreation — mount these from the host in production
# (docker-compose.yml points them at /mnt/user/appdata/taskmanager/ on Unraid).
VOLUME ["/var/lib/postgresql/data", "/app/api/wwwroot/avatars"]

# 8080: the API directly (MCP server, webhooks, or any out-of-band caller).
# 8081: Nginx — the frontend, plus /api and /avatars proxied to the API above.
EXPOSE 8080 8081

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -q -O- http://127.0.0.1:8080/health || exit 1

# s6-overlay (PID 1) runs as root — standard for this supervision model, and
# needed for its own init/fix-attrs steps. Each actual service (Postgres,
# the API, Nginx) drops to the non-root "appuser" itself via s6-setuidgid —
# see docker/s6-overlay/s6-rc.d/*/run and postgres-init/up.
ENTRYPOINT ["/init"]
