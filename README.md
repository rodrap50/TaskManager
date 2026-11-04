# TaskManager

Self-hosted, privacy-first project management app — .NET 10 API + React/Vite frontend, deployed as a single monolithic Docker image (Postgres, the API, and the built frontend, supervised by s6-overlay).

[![Build & Push](https://github.com/rodrap50/TaskManager/actions/workflows/docker-build-push.yml/badge.svg)](https://github.com/rodrap50/TaskManager/actions/workflows/docker-build-push.yml)
[![Release](https://img.shields.io/github/v/release/rodrap50/TaskManager?sort=semver)](https://github.com/rodrap50/TaskManager/releases)
[![GHCR](https://img.shields.io/badge/ghcr.io-taskmanager-blue?logo=github)](https://github.com/rodrap50/TaskManager/pkgs/container/taskmanager)
[![Docker Hub](https://img.shields.io/docker/v/rodrap50/taskmanager?label=docker%20hub&sort=semver)](https://hub.docker.com/r/rodrap50/taskmanager)

## Stack

| Layer | Tech |
|---|---|
| Backend | .NET 10, Clean Architecture (Domain → Infrastructure → Application → API), EF Core + PostgreSQL, MediatR (CQRS) |
| Frontend | React 19 + TypeScript, Vite 6, Tailwind CSS v4 — npm workspaces monorepo, see [`frontend/README.md`](frontend/README.md) |
| Deployment | One multi-target Dockerfile: an all-in-one monolith (Postgres + API + frontend under Nginx + MCP server, supervised by [s6-overlay](https://github.com/just-containers/s6-overlay)), or each service as its own image |

## Running it

```bash
cp .env.example .env
# edit .env: set JWT_SECRET (and POSTGRES_PASSWORD for microservices mode)
```

Pick a deployment mode:

| Mode | Command | Containers |
|---|---|---|
| Monolith | `docker compose up -d` | 1 — `taskmanager` |
| Microservices | `docker compose -f docker-compose.microservices.yml up -d` | 4 — `postgres` (official image), `taskmanager-api`, `taskmanager-web`, `taskmanager-mcp` |

Both use the published images (`ghcr.io/rodrap50/...`, tag from `TASKMANAGER_TAG` in `.env`, default `latest`). Add `--build` to build from source instead.

Either way, the same ports:

- Frontend: `http://localhost:8081`
- API (direct, e.g. for webhooks): `http://localhost:8080`
- MCP server: `http://<host>:8083` — point MCP clients (Claude Desktop, Claude Code, etc.) here over Tailscale/LAN, sending an API token (Admin → MCP Tokens) in an `x-api-token` header.

To archive Done and Cancelled tasks automatically, set `TaskArchiving__AutoArchiveAfterDays` on the API (commented example in both compose files). It is off by default; you can always archive from the board's **Archived** panel.

The API's gRPC port (8082) and Postgres are never published. Data persists under `/mnt/user/appdata/taskmanager/` (monolith) or `/mnt/user/appdata/taskmanager-ms/` (microservices).

## Releases

Version tags (`vX.Y.Z`) trigger a build, push images to both GHCR and Docker Hub, and publish a GitHub Release. `vX.Y.Z-rc.N`/`vX.Y.Z-beta.N` tags publish pre-release images/releases; plain pushes to `main` publish a rolling `beta` image for testing between releases.

Each push to a `release/vX.Y.Z` branch tags the next release candidate automatically (`vX.Y.Z-rc.1`, `-rc.2`, …) and publishes it the same way, including the `rc` image tag. To ship, tag the final commit `vX.Y.Z` yourself; after that the branch refuses further rc pushes.

## Project docs

Planning and architecture live in [`MISSION.md`](MISSION.md), [`ROADMAP.md`](ROADMAP.md), [`TASKS.md`](TASKS.md), and [`TICKETS.md`](TICKETS.md).
