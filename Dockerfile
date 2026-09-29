# syntax=docker/dockerfile:1
#
# One Dockerfile, four images (D05) — pick one with --target:
#   api       TaskManager.API: REST on 8080, internal-only gRPC on 8082
#   web       Nginx serving the frontend, proxying /api and /avatars to $API_UPSTREAM
#   mcp       TaskManager.Mcp: MCP (Streamable HTTP) on 8083
#   monolith  all of the above + Postgres in one container (the default: last stage)
# docker-compose.yml runs the monolith; docker-compose.microservices.yml runs the rest.

# ---------------------------------------------------------------------------
# Stage: build-backend — publish TaskManager.API and TaskManager.Mcp (Release)
#
# Debian-based SDK image, not -alpine: both projects reference
# TaskManager.Grpc.Contracts, whose protoc codegen needs Grpc.Tools' bundled
# native protoc — only shipped as glibc Linux builds (no linux_musl_x64), so it
# can't run under Alpine. Every runtime stage below is still Alpine.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-backend
WORKDIR /src

COPY backend/TaskManager.slnx ./
COPY backend/TaskManager.API/TaskManager.API.csproj TaskManager.API/
COPY backend/TaskManager.Application/TaskManager.Application.csproj TaskManager.Application/
COPY backend/TaskManager.Domain/TaskManager.Domain.csproj TaskManager.Domain/
COPY backend/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj TaskManager.Infrastructure/
COPY backend/TaskManager.Grpc.Contracts/TaskManager.Grpc.Contracts.csproj TaskManager.Grpc.Contracts/
COPY backend/TaskManager.Mcp/TaskManager.Mcp.csproj TaskManager.Mcp/
RUN dotnet restore "TaskManager.API/TaskManager.API.csproj" \
 && dotnet restore "TaskManager.Mcp/TaskManager.Mcp.csproj"

COPY backend/ .
RUN dotnet publish TaskManager.API/TaskManager.API.csproj -c Release -o /app/api /p:UseAppHost=false \
 && dotnet publish TaskManager.Mcp/TaskManager.Mcp.csproj -c Release -o /app/mcp /p:UseAppHost=false

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
# API (see docker/nginx/taskmanager.conf.template) — no production domain
# needs to be known at build time. NOTE: must stay "" (empty), not "/api" —
# ApiClient.ts's endpoint paths already embed "/api/..." themselves.
ENV VITE_API_URL=
RUN npm run build --workspace=packages/web

# ---------------------------------------------------------------------------
# Target: api — TaskManager.API on its own (microservices mode)
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS api
WORKDIR /app

RUN addgroup -S -g 1000 appuser \
 && adduser -S -G appuser -u 1000 -h /app appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
# DatabaseSettings__ConnectionString and Jwt__Secret have no defaults — both are
# required at `docker run`/compose time, per PREP.4's fail-fast Production guard.

COPY --from=build-backend /app/api .
RUN mkdir -p wwwroot/avatars && chown -R appuser:appuser /app

USER appuser
VOLUME ["/app/wwwroot/avatars"]

# 8080: REST (published). 8082: gRPC for the mcp service — keep it unpublished.
EXPOSE 8080 8082

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -q -O- http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "TaskManager.API.dll"]

# ---------------------------------------------------------------------------
# Target: web — the frontend behind Nginx (microservices mode)
#
# nginx-unprivileged: the official NGINX image variant that runs as a non-root
# user, matching the appuser convention everywhere else. Its entrypoint renders
# /etc/nginx/templates/*.template with envsubst at startup.
# ---------------------------------------------------------------------------
FROM nginxinc/nginx-unprivileged:alpine AS web

ENV WEB_ROOT=/usr/share/nginx/html
ENV API_UPSTREAM=http://api:8080

RUN rm -f /etc/nginx/conf.d/default.conf
COPY docker/nginx/taskmanager.conf.template /etc/nginx/templates/taskmanager.conf.template
COPY --from=build-web /src/packages/web/dist /usr/share/nginx/html

EXPOSE 8081

# ---------------------------------------------------------------------------
# Target: mcp — TaskManager.Mcp on its own (microservices mode)
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS mcp
WORKDIR /app

RUN addgroup -S -g 1000 appuser \
 && adduser -S -G appuser -u 1000 -h /app appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8083
# TASKMANAGER_GRPC_URL / MCP_DB_CONNECTION_STRING have no defaults — both point
# at other containers and are set at `docker run`/compose time.

COPY --from=build-backend /app/mcp .
RUN chown -R appuser:appuser /app

USER appuser
EXPOSE 8083

ENTRYPOINT ["dotnet", "TaskManager.Mcp.dll"]

# ---------------------------------------------------------------------------
# Target: monolith (default) — Postgres + API + Nginx + MCP in one container,
# supervised by s6-overlay
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS monolith

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

# Postgres — bundled in this same container, listening on localhost only.
ENV PGDATA=/var/lib/postgresql/data
ENV POSTGRES_USER=postgres
ENV POSTGRES_DB=taskmanager
ENV POSTGRES_PASSWORD=taskmanager_internal

# API — the default connection string below points at the bundled local
# Postgres above. Deliberately NOT the literal PREP.4 dev-fallback string
# (Program.cs's DevFallbackConnectionString), so the Production startup
# guard doesn't misfire — Postgres genuinely IS on localhost in this image.
# Override POSTGRES_PASSWORD, this, and MCP_DB_CONNECTION_STRING together via
# `docker run -e` if pointing at a different/external Postgres instead.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DatabaseSettings__ConnectionString="Host=localhost;Port=5432;Database=taskmanager;Username=postgres;Password=taskmanager_internal"
# Jwt__Secret intentionally has NO default here — required at `docker run`,
# per PREP.4's fail-fast Production guard.

# MCP — reaches the API's gRPC port and its own McpTracking database over
# localhost; mcp-run.sh gives it port 8083.
ENV TASKMANAGER_GRPC_URL=http://localhost:8082
ENV MCP_DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=McpTracking;Username=postgres;Password=taskmanager_internal"

COPY --from=build-backend /app/api /app/api
COPY --from=build-backend /app/mcp /app/mcp
COPY --from=build-web /src/packages/web/dist /app/web
COPY docker/nginx.conf /etc/nginx/nginx.conf
COPY docker/nginx/taskmanager.conf.template /tmp/taskmanager.conf.template
RUN sed -e 's|[$]{WEB_ROOT}|/app/web|g' \
        -e 's|[$]{API_UPSTREAM}|http://127.0.0.1:8080|g' \
        /tmp/taskmanager.conf.template > /etc/nginx/taskmanager.conf \
 && rm /tmp/taskmanager.conf.template
COPY docker/s6-overlay/s6-rc.d /etc/s6-overlay/s6-rc.d
COPY docker/s6-overlay/scripts /etc/s6-overlay/scripts

RUN chmod +x /etc/s6-overlay/s6-rc.d/postgres-init/up \
             /etc/s6-overlay/s6-rc.d/postgres/run \
             /etc/s6-overlay/s6-rc.d/api/run \
             /etc/s6-overlay/s6-rc.d/mcp/run \
             /etc/s6-overlay/s6-rc.d/nginx/run \
             /etc/s6-overlay/scripts/postgres-init.sh \
             /etc/s6-overlay/scripts/postgres-run.sh \
             /etc/s6-overlay/scripts/api-run.sh \
             /etc/s6-overlay/scripts/mcp-run.sh \
             /etc/s6-overlay/scripts/nginx-run.sh \
 && mkdir -p "$PGDATA" /app/api/wwwroot/avatars /var/lib/nginx /var/log/nginx \
 && chown -R appuser:appuser /app /var/lib/postgresql /var/lib/nginx /var/log/nginx /etc/nginx

# Postgres data (D02.2) and avatar uploads (A02) both need to survive
# container recreation — mount these from the host in production
# (docker-compose.yml points them at /mnt/user/appdata/taskmanager/ on Unraid).
VOLUME ["/var/lib/postgresql/data", "/app/api/wwwroot/avatars"]

# 8080: the API directly (webhooks, or any out-of-band caller).
# 8081: Nginx — the frontend, plus /api and /avatars proxied to the API above.
# 8083: MCP (Streamable HTTP). gRPC (8082) and Postgres (5432) stay internal.
EXPOSE 8080 8081 8083

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -q -O- http://127.0.0.1:8080/health || exit 1

# s6-overlay (PID 1) runs as root — standard for this supervision model, and
# needed for its own init/fix-attrs steps. Each actual service (Postgres, the
# API, MCP, Nginx) drops to the non-root "appuser" itself via s6-setuidgid —
# see docker/s6-overlay/s6-rc.d/*/run and postgres-init/up.
ENTRYPOINT ["/init"]
