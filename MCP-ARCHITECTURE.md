# MCP Server Architecture

> **Status:** Active implementation, Sprint 9 (Phase 11). MCP01 is in progress (MCP01.1/MCP01.2 done, not yet signed off); MCP02–MCP07 not started. This doc describes the target architecture all of Sprint 9 builds toward — see [TASKS.md](TASKS.md#-sprint-9--post-deployment-mcp-agent-integration) for the epic board and [TICKETS.md](TICKETS.md#mcp01-tickets--grpc-contracts--api-grpc-surface) for ticket-level acceptance criteria. This file describes the shape of the thing; it doesn't duplicate ticket detail, and isn't updated per-ticket — only when the architecture itself changes.

## Why this exists

`TaskManager.Mcp` lets AI agents (Claude Code, Claude Desktop, and other MCP-capable clients — local or hosted) read and write Projects/Epics/Tasks/Phases, and track their own step-by-step work via a separate `AgentPlan`/`AgentStep` audit trail. Redesigned 2026-09-08 from an earlier Node.js/TypeScript/REST-only sketch (M01–M04, superseded) once Epic existed as a real entity and the `ApiToken` auth scheme existed to scope agent access.

## Topology

```
                    ┌─────────────────────────────────────────────┐
                    │         AI client (Claude, etc.)             │
                    └───────────────────┬───────────────────────────┘
                                         │ MCP protocol (Streamable HTTP)
                                         │ bearer token = an ApiToken (MCP02)
                    ═════════════════════▼═══════════════════════  ← Tailscale/LAN-exposed
                    │           TaskManager.Mcp (MCP03)            │
                    │  ─────────────────────────────────────────  │
                    │  Tools (MCP04, MCP05):                       │
                    │   • Hierarchy tools → forward token via gRPC │
                    │   • AgentPlan/Step tools → McpDbContext only │
                    │  No auth logic of its own — just forwards    │
                    │  the caller's token and maps gRPC errors     │
                    └─────┬─────────────────────────────┬─────────┘
                          │ gRPC (token as               │ EF Core
                          │ call credentials)             │
              ┌───────────▼──────────┐         ┌─────────▼──────────┐
              │  TaskManager.API      │         │  McpTracking DB    │
              │  2nd Kestrel port     │         │  (own Postgres     │
              │  cleartext HTTP/2     │         │  database, no FK   │
              │  (MCP01.2 — done)     │         │  to the monolith)  │
    ══════════╪═══════════════════════╪═════════╪═════════════════ ═ ← internal Docker network only
              │  *GrpcService classes │         │
              │  call the SAME        │         │
              │  MediatR handlers the │         │
              │  REST controllers do  │         │
              └───────────┬───────────┘         └─────────────────────┘
                          │
              ┌───────────▼───────────┐
              │  taskmanager Postgres │  ← the existing app DB
              └────────────────────────┘
```

## The pieces, per epic

| Epic | What it is | File/project scope |
|---|---|---|
| **MCP01** | `TaskManager.Grpc.Contracts` (shared proto contract) + `*GrpcService` classes on `TaskManager.API` — a second transport over the *same* Application-layer MediatR handlers the REST controllers already call. No new business logic. | `backend/TaskManager.Grpc.Contracts/`, `backend/TaskManager.API/Grpc/` |
| **MCP02** | `ApiToken` scoping — `IsReadOnly`/`ExpiresAt`/`RateLimitPerMinute` + a write-guard middleware and rate limiter that cover REST *and* gRPC identically (middleware, not MVC filters, since filters don't run for gRPC). This is the auth mechanism MCP tools ride on. | `backend/TaskManager.Domain/Entities/ApiToken.cs`, `backend/TaskManager.API/Authentication/`, `backend/TaskManager.API/Middleware/` |
| **MCP03** | Standalone `TaskManager.Mcp` project (deliberately un-layered — "two entities and a handful of tools don't earn" Domain/Application/Infrastructure split). Hosts the MCP server (`AddMcpServer().WithHttpTransport()`), its own `McpDbContext`/`McpTracking` DB for `AgentPlan`/`AgentStep`, and a `GrpcClients/` wrapper that forwards the inbound bearer token as gRPC call credentials. | `backend/TaskManager.Mcp/` (new) |
| **MCP04** | The actual MCP tools for the Projects/Epics/Tasks/Phases hierarchy — thin wrappers around MCP03's gRPC client wrapper. No token validation here; a bad token surfaces as `UNAUTHENTICATED` → mapped to an MCP-level auth error, a read-only token's write attempt surfaces `PERMISSION_DENIED` → mapped cleanly. | `backend/TaskManager.Mcp/Tools/` |
| **MCP05** | Agent-plan audit-trail tools (`create_agent_plan`, `add_agent_step`, etc.) — these talk to `McpDbContext` **directly**, no gRPC hop, no dependency on `TaskManager.API` at all. Separate data path from MCP04. | `backend/TaskManager.Mcp/Tools/` |
| **MCP06** | Frontend admin screen for managing these scoped tokens (flagged as possibly overlapping/superseding the deferred AUTH03 — unresolved, see below). | `frontend/packages/web/src/screens/admin/McpTokensScreen/` (new) |
| **MCP07** | Dockerfile + `docker-compose.yml` entry. Only the MCP's own HTTP port is published (Tailscale/LAN); the gRPC leg and `McpTracking` Postgres connection stay internal-only on the Docker network — verified in MCP07.3 by literally trying to reach the gRPC port from outside the network and confirming it fails. | `backend/TaskManager.Mcp/Dockerfile` (new), `docker-compose.yml` |

## Key architectural decisions

1. **Two separate data paths from `TaskManager.Mcp`**: hierarchy data (Projects/Epics/Tasks/Phases) goes out over gRPC to the monolith; agent-plan tracking data goes straight to MCP's own `McpTracking` database. No cross-referencing between the two beyond `AgentPlan.ProjectId` as a loose (non-FK) pointer.
2. **No duplicated auth logic in MCP.** Every tool just forwards the caller's token as-is; all real enforcement (expiry, read-only, rate limit) lives in MCP02's middleware on `TaskManager.API`, shared identically by REST and gRPC.
3. **gRPC surface is never publicly reachable** — not proxied through nginx (confirmed for MCP01.2), not published in `docker-compose.yml` (MCP07.2), and MCP07.3 actively tests that from-outside connections to it fail.
4. **`TaskManager.Mcp` is intentionally NOT layered** like the rest of the backend — one flat project, a deliberate deviation from the monolith's Clean Architecture convention (Pillar 3 in [MISSION.md](MISSION.md#-core-pillars)), justified by its small scope (two entities, a handful of tools).
5. **Kestrel gotcha (MCP01.2):** adding the second gRPC endpoint via explicit `ListenAnyIP` in code disables `ASPNETCORE_URLS`-based endpoint binding entirely — the REST endpoint has to be re-declared in code alongside it. Verified live; see `Program.cs`'s comment at the Kestrel config block for the exact behavior observed.

## Open question, not yet resolved

**MCP06 vs. AUTH03:** MCP06 (new MCP-specific token admin screen) may fully supersede the already-deferred AUTH03 (generic `ApiToken` admin screen) once MCP06 ships, since MCP06 covers the same tokens plus the new scoping fields. Flagged in TICKETS.md for a decision when MCP06 is picked up — not resolved automatically.

---

See also: [MISSION.md](MISSION.md) for the project's overall architecture and pillars, [ROADMAP.md](ROADMAP.md#phase-11-post-deployment--mcp-server-agent-integration-layer) for Phase 11's place in the broader roadmap.
