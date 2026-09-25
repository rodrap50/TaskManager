# Implementation Roadmap — HomeAssistant Project Tracker

> Phase-by-phase execution plan. For project goals, pillars, and architecture see [MISSION.md](MISSION.md).
> Epic sprint board: [TASKS.md](TASKS.md) · Active tickets: [TICKETS.md](TICKETS.md) · Completed tickets: [CompletedTickets.md](CompletedTickets.md)

---

## Phase 1: Backend Foundations (.NET 10)

- [x] **B01: Domain Entities & Identity**
  - Framework-agnostic domain models: `Project.cs`, `ProjectTask.cs`, `Phase.cs` (`ProjectPhase`), `AppUser.cs`.
  - Tracking fields: `AssignedUserId`, `SecondaryAssigneeId`, `Priority` (Low/Medium/High/Critical), `Scope` (Epic/Project/DailyTask), `ExternalMetadata` JSON dictionary for external tool footprints.
  - `AuditInfo` immutable value object with `CreatedAt`, `UpdatedAt`, and `RowVersion` optimistic-concurrency token.
- [x] **B02: Infrastructure & EF Core**
  - `AppDbContext` targeting PostgreSQL via Npgsql.
  - Full Fluent API: `AuditInfo` as owned entity (table-split), `ExternalMetadata` as `jsonb` column with value comparer, all FK relationships with correct delete behaviours (`Cascade`, `Restrict`, `SetNull`), unique indexes on `Username` and `Email`.
  - `InitialCreate` migration generated and verified against live PostgreSQL container.
  - `DataSeeder` seeds well-known system placeholder user (`00000000-0000-0000-0000-000000000001`) on first boot.
  - Auto-migration on startup wired into `Program.cs`.
- [x] **B03: Repository Layer**
  - `IRepository<T>` generic interface + `IUnitOfWork` in the Application layer.
  - `GenericRepository<T>` base + 4 concrete repositories (`ProjectRepository`, `ProjectTaskRepository`, `ProjectPhaseRepository`, `UserRepository`) + `UnitOfWork` in Infrastructure.

---

## Phase 2: Backend API & Service Layer

- [x] **B04: Business Services & DTOs**
  - Response DTOs: `ProjectDto`, `ProjectTaskDto`, `ProjectPhaseDto`, `AppUserDto` with static `ToDto()` mapping via `MappingExtensions`.
  - FluentValidation pipeline behavior + validators for all write commands.
  - Full CQRS handlers: Projects (GetAll/GetById/Create/Update/Archive/Restore), Tasks (GetByProject/GetById/Create/Update/Transition), Phases (GetByProject/Create/Update/Delete), Users (GetAll/GetById/Create).
  - `IPasswordHasher` interface + BCrypt implementation.
  - Global exception middleware: `ValidationException → 400`, `InvalidOperationException → 422`.
- [x] **B05: Traditional Controllers**
  - `ProjectsController` — full CRUD + archive/restore.
  - `TasksController` — full CRUD + status transition.
  - `PhasesController` — full CRUD.
  - `UsersController` — GET all/by-id, POST create.
- [ ] **B05b: Integration Gateway** *(post-MVP)*
  - `IntegrationController` with API-token-authenticated webhook endpoints for inbound payloads from Home Assistant and external automation scripts. Will consume the `ApiToken` infrastructure built in Phase 6's AUTH01 rather than inventing its own token scheme.

---

## Phase 3: Frontend Monorepo Setup

- [x] **F01: Project Initialization** — Vite + React + TypeScript + Tailwind CSS.
- [x] **F02: Shared API Client** — `@taskmanager/shared`: all TypeScript DTOs, axios instance targeting `VITE_API_URL`, named exports for all endpoints.
- [x] **F03: Shared UI Component Library** — `@taskmanager/ui`: `TaskCard`, `PriorityBadge`, `StatusIndicator`, `AssigneeAvatar` — pure presentational, no API dependencies.
- [x] **F04: npm Workspace Monorepo Split**
  - `@taskmanager/web` — always-online WebUI: direct axios calls via `@taskmanager/shared`, no SQLite or Capacitor.
  - `@taskmanager/mobile` — preserved Capacitor + `jeep-sqlite` + `DatabaseService` + mutation queue sync engine; deferred to post-MVP mobile phase.
  - Root `frontend/package.json` as workspace host; `dev`/`build` scripts scoped to `packages/web`.

> **Note:** The original single-app approach hit a dev-environment WASM crash (`jeep-sqlite` MIME type issue). The monorepo split is the resolution — `packages/web` never loads WASM.

---

## Phase 4: WebUI Presentation Layer

- [x] **U01: Board & List Dashboard**
  - Kanban view (columns: Backlog, In Progress, Blocked, Done) with drag-and-drop; status transitions call `transitionTask()` directly.
  - Prioritized list view with task detail bottom panel; field changes call `updateTask()` / `transitionTask()` directly.
  - View toggle persisted to `localStorage`. Project selector with create modal.

> **U03 (Gesture-Driven Interactions)** was originally scoped here but is fully deferred — its only home now is [Post-MVP Backlog → Mobile App Phase](#post-mvp-backlog).

---

## Phase 4b: Desktop-Only Administration UI

- [x] **A01: Admin Panel Shell & Guardrails** — ✅ Done. Desktop-only layout in `packages/web` with side-navigation and a live-resize media-query lock (`AdminGuard.tsx`, sub-1024px viewports blocked entirely, no partial admin UI ever renders). Entry point is an `isAdmin`-gated button in `FloatingPill`, fully absent for non-admins. Verified directly against source 2026-08-23 — was built but never marked done in these docs until now. See [CompletedTickets.md](CompletedTickets.md#a01-tickets--admin-panel-shell--guardrails) (A01.1–A01.3).
- [x] **A02: User Management Portal** — ✅ Done. Create, update, and manage `AppUser` accounts, roles, and avatar assignments. `UsersController` only exposed `GetUsers`/`GetById`/`CreateUser` — status/role/avatar endpoints didn't exist despite the domain layer already supporting them, so this included real backend work (`PUT /api/users/{id}/active`, `PUT /api/users/{id}/admin`, `POST /api/users/{id}/avatar`). See [CompletedTickets.md](CompletedTickets.md#a02-tickets--user-management-portal) (A02.1–A02.6).

> **A03 (Webhook & Integration Manager)** pushed post-MVP per user direction — see [Post-MVP Backlog → Platform Extensions](#post-mvp-backlog). It was already hard-blocked by B05b (also post-MVP, Phase 2) regardless, so nothing here was actually deployable pre-MVP anyway.

---

## Phase 5: Data Model — Epic Entity Restructure & DailyTask Simplification

> Phase 5 is now fully done (DM01/DM02 signed off 2026-08-26, DM03 signed off 2026-08-26) — see [EPIC-ENTITY-PLAN.md](EPIC-ENTITY-PLAN.md) for the researched, approved plan that started this off (its frontend file paths were corrected during ticketing, since it predates DS05/DS06). Expanded to also cover DailyTask projects becoming a simplified todo list, since both touch `ProjectScope`/project-type behavior. Fully ticketed with Fibonacci story points (capped at 5) — see [CompletedTickets.md](CompletedTickets.md#dm01-tickets--backend-epic-entity).

- [x] **DM01: Backend — Epic entity** — New `Epic` domain entity as a child of `Project` (sibling of the existing `ProjectPhase`, not a replacement), full CQRS CRUD mirroring the Phase pattern exactly (Domain/Infrastructure/Application/API), single optional `EpicId` FK on `ProjectTask`, `ProjectScope` enum cleanup (removes the `Epic` value — an "Epic" is no longer a kind of `Project`), EF migration + dev DB reset. Confirmed free-form like Phase: no fixed/hardcoded set, full per-project CRUD, optional per task.
- [x] **DM02: Frontend — Epic entity consumption** — Shared `EpicDto` + API client CRUD functions, new `useEpics` hook mirroring `usePhases`, an Epic picker in `TaskDetailPanel` mirroring the existing Phase picker — **scope-gated to `Project`-scope projects only**, hidden entirely for DailyTask projects. Removed `'Epic'` from `CreateProjectModal`'s scope options. Expanded post-ship with a full Epics management panel (create/rename/delete) and randomly-assigned, per-project-excluded badge colors for both Kanban and List views — see [CompletedTickets.md](CompletedTickets.md#dm02-tickets--frontend-epic-entity-consumption) for the full history.
- [x] **DM03: DailyTask projects become a simplified todo list** — DailyTask-scope projects show only a list view (no Kanban/`ViewToggle` at all), tasks use a simple complete/incomplete model (reuses the existing `TaskStatus` enum: checking = `Done`, unchecking = `Backlog` — no new field, and it lines up with the existing Done-can-only-reopen-to-Backlog rule from BUG-07), and DailyTask projects get no Phases at all — extending naturally to no Epics either via DM02's scope-gating. UI-layer enforcement only, not server-side. See [CompletedTickets.md](CompletedTickets.md#dm03-tickets--dailytask-simplified-todo-list) for the full history.

---

## Phase 6: Identity & Access (MVP-Required)

> New MVP-required phase — inserted after Phase 5. Originally scoped because the app had **zero authentication** (no login, no JWT, no session — confirmed by codebase audit) and no concept of project membership (only per-*task* `AssignedUserId`/`SecondaryAssigneeId` exist). **Phase 6 is now fully done** — AUTH01/AUTH02 (authentication, including first-run admin bootstrap) and PM01/PM02 (project membership, including a task-assignment restriction added mid-sprint) all shipped. Phase 7's weighted voting is now unblocked.

- [x] **AUTH01: Backend — Authentication & API Tokens** — ✅ Done. JWT login endpoint validating against the existing BCrypt password hash (`IPasswordHasher`); JWT Bearer middleware wired globally, `[Authorize]` required on all controllers by default; new `ApiToken` entity for non-human/service-account access (admin generate/list/revoke) for eventual external services (Home Assistant, automation scripts) to call the API without a human login; a second auth scheme/middleware accepting a valid `ApiToken` header as an alternative to a JWT. Also closed a privilege-escalation gap discovered mid-implementation: `POST /api/users` is now admin-only (previously any authenticated user could create accounts, including setting `isAdmin: true`). See [CompletedTickets.md](CompletedTickets.md#auth01-tickets--backend-authentication--api-tokens).
- [x] **AUTH02: Frontend — Login & Session** — ✅ Done. Login screen (username/password), axios interceptor attaching the bearer token to every `@taskmanager/shared` request, `AuthContext`/current-user state (id, username, `isAdmin`), route guard redirecting unauthenticated users to login, logout. Scope expanded mid-sprint to close the bootstrapping gap AUTH01's admin-only restriction created: a one-time `POST /api/auth/setup` (DB-level-guarded against ever creating a second admin, even under concurrent submission) plus a `SetupScreen` the frontend shows instead of login only while no admin exists yet. See [CompletedTickets.md](CompletedTickets.md#auth02-tickets--frontend-login--session).
- [x] **PM01: Backend — Project Membership** — ✅ Done (signed off 2026-08-24). New `ProjectMember` join entity (`Project` ↔ `AppUser`, flat membership — no per-project roles; admin powers reuse the existing global `AppUser.IsAdmin`); `Project.AddMember`/`RemoveMember`; whoever creates a project is automatically added as a member via a new `ICurrentUserService`; CQRS + controller for add/remove/list members. Found and fixed a real EF Core bug along the way (`Guid`-key convention misreading a new child add as an update — fixed with `ValueGeneratedNever()`). See [CompletedTickets.md](CompletedTickets.md#pm01-tickets--backend-project-membership).
- [x] **PM02: Frontend — Project Membership UI** — ✅ Done (signed off 2026-08-25). "Members" panel reachable from within the project itself (not the admin panel); list current members with avatars; add/remove members via a search over existing `AppUser`s (`useUsers()`). Expanded mid-sprint (PM02.3): task assignment is now restricted to actual project members, both frontend (assignee pickers source from the project roster) and backend (async DB-backed validator rules). See [CompletedTickets.md](CompletedTickets.md#pm02-tickets--frontend-project-membership-ui).

---

## Phase 7: Weighted Priority Scoring (MVP-Required)

> Depends on Phase 6 (needs a real logged-in identity to attribute votes to, and `ProjectMember` to know who's eligible to vote) — now done. Also depends on Phase 5 having landed cleanly: `TaskCard`/`ListView`/`KanbanBoard` all moved location during Phase 5's DS-series and DM-series work, and Phase 5 introduced a new `TodoList` component (`DailyTask`-scope projects) that these tickets need to cover. Confirmed with the user: voting/weighted scoring applies uniformly across all project scopes — `DailyTask` tasks are weighted exactly like `Project`-scope tasks, no scope-gating (unlike Phase/Epic in Phase 5). **Phase 7 is now fully done (PRI01/PRI02/PRI03 signed off 2026-08-28)** — see [CompletedTickets.md](CompletedTickets.md#pri01-tickets--backend-voting--weighted-scoring).

- [x] **PRI01: Backend — Voting & Weighted Scoring** — New `TaskVote` entity (unique per task+user, 1–10 scale) and `Project.CriticalityScore` (1–10, admin-settable) + EF migration; weighted score computation `ceiling((PriorityNumeric + AvgUserVote + CriticalityScore) / 3)` where `PriorityNumeric` maps the existing `Priority` enum onto the same 1–10 scale (Low=3, Medium=7, High=9, Critical=10) and every average/rounding step in the chain always rounds up, never down; cast/update-vote endpoint restricted to `ProjectMember`s, one editable vote per user per task, attributed to the current authenticated user; admin-settable `Project.CriticalityScore`. Already scope-agnostic by design — no `DailyTask` special-casing needed here. See [CompletedTickets.md](CompletedTickets.md#pri01-tickets--backend-voting--weighted-scoring).
- [x] **PRI02: Frontend — Voting UI** — 1–10 voting control wired into the one shared `TaskDetailPanel`, automatically usable from Kanban, List view, and `TodoList` alike; computed weighted score displayed on `TaskCard` (bottom-right corner — the top-right is already taken by Phase 5's epic badge), `ListView` rows, and `TodoList` rows. Expanded mid-sprint with an ad-hoc admin screen (PRI02.5) for setting `Project.CriticalityScore` — see [ROADMAP.md](ROADMAP.md#post-mvp-backlog)'s "Project Settings page" backlog item for its eventual real home. See [CompletedTickets.md](CompletedTickets.md#pri02-tickets--frontend-voting-ui).
- [x] **PRI03: Default Sorting** — Board/List/`TodoList` views default-sort tasks by weighted score descending; Project selector default-sorts projects by `CriticalityScore` descending. Full sorting/filtering *controls* (as opposed to just default order) are a post-MVP backlog item — see below. See [CompletedTickets.md](CompletedTickets.md#pri03-tickets--default-sorting).

---

## Phase 8: Bug Fixes & Deployment Prep

> **Phase 8 is now fully done (PREP signed off 2026-08-30, BUGS already done)** — every known bug is resolved and the app is hardened for production (locked-down CORS, a `/health` endpoint, confirmed env-var wiring for the production build and the database connection string/JWT secret, confirmed Serilog sinks, and the legacy pre-monorepo `frontend/src/` tree removed). Phase 9's containerization work is now unblocked.

- [x] **BUGS: Resolve known bug tickets** — ✅ Done. All seven bugs resolved: BUG-01/04/05/06 (Sprint 1, app-breaking) and BUG-02/03/07 (Sprint 3, UX polish — BUG-02/03 fixed as part of DS05.2/DS05.4, BUG-07 fixed as part of DS03.4). No open bugs remain. Full history archived at [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking) and [CompletedTickets.md](CompletedTickets.md#sprint-3-bug-tickets--ux-polish-pre-deployment).
- [x] **PREP: Production readiness** — ✅ Done. Locked CORS to a DB-backed, admin-editable allow-list (removed `AllowAnyOrigin`); added a `/health` endpoint; confirmed `VITE_API_URL` production build wiring (documented in `.env.example` since the real production URL is deployment-specific and depends on Phase 9's D03); reviewed connection-string/secrets handling and added a Production startup guard that fails fast if the DB connection string or JWT secret aren't configured; confirmed the Serilog console sink for container stdout and added friendly `SEQ_URL`/`LOG_FILE_PATH` env vars; deleted the legacy pre-monorepo `frontend/src/` tree and its orphaned root config. See [CompletedTickets.md](CompletedTickets.md#prep-tickets--production-readiness--environment-config).

---

## Phase 9: Containerization & Initial Deployment

> Redefined per user direction (2026-08-30): rather than three separate images orchestrated via docker-compose (API, frontend, Postgres), the production deployment is a single monolithic container — Postgres, the API, and the built frontend all run together under one process supervisor (s6-overlay), for simpler operation on a single-host Unraid deployment. **Phase 9 is now fully done (signed off 2026-08-31)** — the monolithic image builds, boots, and has been verified end-to-end live (browser-driven first-run setup, project creation, direct-port API access, and data persisting across container recreation). Phase 10's quick-capture work and Phase 11's MCP server work are now unblocked. See [CompletedTickets.md](CompletedTickets.md#d01d03-tickets--monolithic-production-docker-image) for the full ticket-by-ticket history (D01.1–D01.2, D02.1–D02.2, D03.1–D03.2).

- [x] **D01: Build Stages** — Dockerfile stages that produce a published .NET API build and a built frontend bundle (calling the API via a relative path, so no production domain needs to be known at build time — implemented as an empty-string `VITE_API_URL`, not the originally-planned `/api`, since `ApiClient.ts` already prefixes its own `/api`/`/avatars` paths).
- [x] **D02: Monolithic Runtime Base** — Final-stage image bundling Postgres, the .NET runtime, and Nginx together on Alpine, supervised by s6-overlay; non-root user; Postgres data on a persistent volume.
- [x] **D03: Assemble, Wire & Verify** — Nginx serves the frontend and proxies `/api/*` locally; the API is also separately exposed on its own port for direct access (future MCP/webhook consumers); startup order (Postgres → API → Nginx); Production secrets via env var per PREP.4's guard; `/health`-based `HEALTHCHECK`; volumes for Postgres data and avatar uploads at `/mnt/user/appdata/taskmanager/`; verified end-to-end including data persisting across container recreation. *(MCP service still added later, as its own separate container, in Phase 11's MCP07.)*

---

## Phase 10: Post-Deployment — Quick Capture

- [ ] **U02: Quick-Capture FAB Modal** — Floating Action Button → bottom sheet → task creation form. Fields: title, project, priority, due date, assignee. Submit calls `createTask()` from `@taskmanager/shared`, then refetches.

---

## Phase 11: Post-Deployment — MCP Server (Agent Integration Layer)

> Redesigned per user direction (2026-09-08), superseding the original M01–M04 sketch (Node.js/TypeScript, REST-only, Projects+Tasks scope) — that sketch predated Epic existing as a real entity (Phase 5) and predated the `ApiToken` auth scheme (Phase 6). `TaskManager.Mcp` is now C#, talks to `TaskManager.API` over gRPC (internal-only Docker network), and exposes itself to AI clients over remote Streamable HTTP so hosted/browser clients — not just local stdio ones — can reach it. Full hierarchy coverage (Projects, Epics, Tasks, Phases) and a separate agent-plan audit trail (`AgentPlan`/`AgentStep`, its own `McpTracking` Postgres database) are both new scope. See [TASKS.md](TASKS.md#-sprint-9--post-deployment-mcp-agent-integration) for the epic board (MCP01–MCP07) and [TICKETS.md](TICKETS.md#mcp01-tickets--grpc-contracts--api-grpc-surface) for tickets.

- [x] **MCP01: gRPC Contracts + API gRPC Surface** — New `TaskManager.Grpc.Contracts` project (`.proto` files for Projects/Epics/Tasks/Phases, messages mirroring the existing DTOs field-for-field, service definitions mirroring the REST surface 1:1). `TaskManager.API` gains a second, internal-only Kestrel HTTP/2 endpoint and `ProjectsGrpcService`/`EpicsGrpcService`/`TasksGrpcService`/`PhasesGrpcService` implementations calling the same MediatR commands/queries the REST controllers already call — a second transport over the existing Application layer, no duplicated business logic.
- [ ] **MCP02: ApiToken Scoping** — `ApiToken` gains `IsReadOnly`, `ExpiresAt`, `RateLimitPerMinute` (+ EF migration). `ApiTokenAuthenticationHandler` rejects expired tokens and adds `isReadOnly`/`rateLimitPerMinute` claims. New write-guard middleware (not an MVC action filter, so it also covers gRPC) blocks non-safe requests from read-only tokens. New in-process rate-limiting middleware (`Microsoft.AspNetCore.RateLimiting`, partitioned by `apiTokenId` claim) covers REST and gRPC alike.
- [ ] **MCP03: Scaffold `TaskManager.Mcp`** — New standalone project (deliberately not layered like the monolith — two entities and a handful of tools don't earn Domain/Application/Infrastructure/API separation). `Program.cs` wires `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()` + `app.MapMcp()`. Its own `McpDbContext` (EF Core) against a new `McpTracking` Postgres database (same Postgres instance, separate database, no FK coupling to `TaskManager.Domain`) with `AgentPlan`/`AgentStep` entities and an initial migration. A `GrpcClients/` wrapper around the generated `TaskManager.Grpc.Contracts` stubs, forwarding the inbound MCP bearer token as gRPC call credentials.
- [ ] **MCP04: Hierarchy MCP Tools** — `list_projects`, `get_project`, `list_epics`, `create_epic`, `update_epic`, `list_phases`, `list_tasks`, `get_task`, `create_task`, `update_task`, `transition_task`. No token logic of their own — a bad/expired/revoked token surfaces as an `UNAUTHENTICATED` gRPC status mapped to an MCP-level auth error; a mutating tool called with a read-only token surfaces `PERMISSION_DENIED` cleanly.
- [ ] **MCP05: Agent Plan Tracking MCP Tools** — `create_agent_plan`, `get_agent_plan`, `add_agent_step`, `complete_agent_step`, `fail_agent_step`, `complete_agent_plan`. Read/write `McpDbContext` directly — no network hop, no dependency on `TaskManager.API`. `AgentPlan` is scoped to a required `ProjectId`, mirroring how `Epic`/`ProjectPhase` both hang off `Project`.
- [ ] **MCP06: Frontend — MCP Tokens Admin Screen** — New `McpTokensScreen` (mirrors `AllowedOriginsScreen`'s shape: table + add form + per-row revoke), extended for `IsReadOnly`/`ExpiresAt`/`RateLimitPerMinute`. Raw token shown once at creation, never retrievable again. `AdminShell.tsx` gains an `'mcpTokens'` screen + nav entry; `ApiClient.ts`'s `ApiTokenDto`/`CreateApiTokenRequest` gain the new fields.
- [ ] **MCP07: Containerize & Wire into docker-compose** — `TaskManager.Mcp` Dockerfile + a `docker-compose.yml` service entry. The MCP HTTP endpoint is exposed over Tailscale/LAN; the gRPC leg to `TaskManager.API` and the `McpTracking` Postgres connection stay internal-only on the Docker network, same deployment posture as the rest of the stack.

---

## Post-MVP Backlog

### Mobile App Phase
- [ ] **MOB01** — Activate `packages/mobile`: Capacitor build for iOS/Android (WASM MIME issue is non-issue on device).
- [ ] **U03** — Gesture-driven interactions (swipe-to-complete, swipe-to-assign) in `packages/mobile`.
- [ ] **MOB02** — Offline-first sync: full mutation queue replay, conflict resolution.
- [ ] **MOB03** — Push notifications via Capacitor Push plugin.
- [ ] **MOB04** — Migrate the Kanban board's drag-and-drop (`KanbanBoard.tsx`) from native HTML5 DnD to `@dnd-kit/core`, for touch support and keyboard accessibility. Native HTML5 DnD (shipped as part of DS03.2) works fine for the current desktop-only web MVP — this migration only becomes valuable once mobile/touch use is in scope. Decision confirmed with the user in place of DS03.4's original acceptance criterion (see [CompletedTickets.md](CompletedTickets.md#ds03-tickets)).

### Platform Extensions
- [ ] **AUTH03: Admin — API Token Management UI** — Generate/revoke `ApiToken`s from the admin panel; token value shown once at creation (copy-to-clipboard), never displayed again. Absorbs the token-management half of A03 (Webhook & Integration Manager, Phase 4b) — A03 becomes the consumer of this backend rather than duplicating it. Pushed post-MVP per user direction — AUTH01's backend already ships standalone and is usable via direct API calls; this is UI-only remaining scope. Fully specced and ready to pick up — see [TICKETS.md](TICKETS.md#auth03-tickets--admin-api-token-management-ui).
- [ ] **A03: Admin — Webhook & Integration Manager** *(Phase 4b)* — Webhook URL config and execution log. Pushed post-MVP per user direction; was also already hard-blocked by B05b (below), so it couldn't have shipped pre-MVP regardless. Not yet ticketed.
- [ ] **Admin: Transfer sole-admin status** — Discovered during A02: the app enforces exactly one admin, always (a DB-level constraint from AUTH02.3). But A02's promote/revoke endpoints are independent toggles — promote is blocked once an admin exists, revoke is blocked when it's the last one — so there is currently no way to ever change who that one admin is. Needs an atomic "transfer admin" operation (revoke + promote in a single transaction) exposed via the admin UI. Not yet ticketed.
- [ ] **Admin: Edit user profiles + self-service profile editing** — Two related capabilities: (1) admin can edit another user's display name and email, and reset their password, from A02's User Management Portal; (2) every user gets a "My Account" screen to edit their own display name, email, and change their own password — self-service editing doesn't exist anywhere in the app today (only the admin-facing pieces were in A02's Sprint 2 scope). Needs: backend endpoints distinguishing admin-on-behalf-of edits from self-service ones (a self-directed password change should require the current password; an admin-initiated reset shouldn't), a new "My Account" screen (ties into the same missing per-user Settings concept already flagged by the font backlog item below), and additions to A02's `UserManagementScreen` for editing name/email and triggering a reset. Not yet ticketed.
- [ ] **Project Settings page (per-project, admin-scoped)** — A real settings surface for individual projects, replacing the growing pile of one-off interim admin screens each new per-project setting keeps generating. First concrete need: `Project.CriticalityScore` (PRI01.4) currently only has an **interim** `ProjectCriticalityScreen` under the Admin section — a flat list of every project with a 1–10 input, explicitly built as a stopgap (see [CompletedTickets.md](CompletedTickets.md#pri02-tickets--frontend-voting-ui), PRI02.5) pending this page existing. When built, PRI02.5's control should migrate here and the interim screen retire. Likely candidates to also land here eventually: **Configurable card/task layout**, if the open global-vs-per-project question above resolves to per-project; a per-project home for **Custom Board Columns** (already logged in UI/UX Enhancements below). Not yet ticketed.
- [ ] **RBAC** — Fine-grained role-based access control beyond Phase 6's flat `ProjectMember` + global `AppUser.IsAdmin` model. Per-project roles (e.g. owner/contributor/viewer) with distinct permissions on voting, editing, and admin actions. Explicitly deferred post-MVP per user direction.
- ~~**Docker: Disable Bundled Postgres**~~ — Graduated out of the backlog (2026-09-01), now ticketed as **D04** (new Sprint 10) — see [TASKS.md](TASKS.md#d04) and [TICKETS.md](TICKETS.md#d04-tickets--disable-bundled-postgres-toggle).
- [ ] **B06** — Background Automation Engine (`BackgroundService` queue runner for async webhook events; prerequisite for B05b).
- [ ] **Home Assistant Integration** — Native HA integration beyond webhooks.
- [ ] **BFF** — Backend for Frontend: per-client optimized API layer once web and mobile needs meaningfully diverge.
- [ ] **Admin: Configurable Logging** — UI settings for log level, retention, and sink targets at runtime.
- [ ] **Admin: Custom Webhook Builder** — Define outbound webhook URLs, target projects, and payload templates from the admin UI.
- [ ] **In-app "needs your vote" notifications** — In-app only (not push/email/SMS) notification system surfacing tasks the current user hasn't yet weighed in on, per Phase 7's voting system (PRI01–03) — e.g. a bell icon/badge with a count, opening a dropdown/panel listing tasks where the user is a `ProjectMember` but hasn't cast a `TaskVote` yet. **Depends on Phase 7 shipping first** — no voting system, no gap to surface. Needs: a backend query joining eligible tasks (via `ProjectMember`) against existing `TaskVote` rows to find the un-voted set, a notification UI component (bell + badge + dropdown, consistent with the existing floating-pill chrome), and a scope decision — every eligible task ever, or only recently-created/still-open ones (an old stale task probably shouldn't nag forever). Distinct from Email/SMS Notifications below, which are external channels.
- [ ] **Email Notifications** — Assignee alerts on task create/update/complete.
- [ ] **SMS Notifications** — High-priority task assignment alerts.
- [ ] **MCP: Project-level write tools (`create_project`/`update_project`)** — Identified during MCP04 (Hierarchy MCP Tools). `projects.proto` (added in MCP01) only defines `ListProjects`/`GetProject` — there's no `CreateProject`/`UpdateProject` RPC, even though the REST API's `ProjectsController` has both (`POST /api/projects`, `PUT /api/projects/{id}`). Because MCP04.1's `TaskManager.Mcp/Tools/ProjectTools.cs` rides entirely on the gRPC surface, an MCP-connected agent can currently list/read projects but not create or update them — project creation/editing stays REST-only (driven through the web UI), while epics, tasks, and phases are already agent-writable. Not a gap in MCP04 itself — its own acceptance criteria only ever scoped `list_projects`/`get_project`, and the gRPC contract never had Create/UpdateProject RPCs to wrap in the first place. Future work, if picked up: add `CreateProject`/`UpdateProject` RPCs to `projects.proto`; implement them in `TaskManager.API/Grpc/ProjectsGrpcService.cs` (mirroring the existing `EpicsGrpcService.CreateEpic`/`UpdateEpic` pattern); add matching wrapper methods to `TaskManager.Mcp/GrpcClients/ProjectsGrpcClient.cs`; expose `create_project`/`update_project` MCP tools in `TaskManager.Mcp/Tools/ProjectTools.cs` (mirroring `EpicTools.cs`) — extending agent write-access up to the project level. Confirmed with the user (2026-09-25) to leave as-is for now — not ticketed, not scheduled into a sprint.

### Content & Editing
- [ ] **Rich text description editor** — Replace the plain `<textarea>` used for Project (and later Task) descriptions with a rich text editor (formatting, lists, links). More than a UI swap: needs a storage-format decision (HTML/Markdown/JSON) and every place a description renders needs to handle it, not just the input.
- [ ] **Project rules section** — Optional structured "rules"/conventions section on a Project, aimed at coding projects (lint conventions, style guide notes, agent-facing constraints). Needs a schema decision (new field vs. child entity) beyond a UI change.

### UI/UX Enhancements
- [ ] **Expandable FloatingPill header** — Let the top-right `FloatingPill` (see [MISSION.md](MISSION.md#-design--style-decisions)) extend into a full-width header bar instead of staying a fixed pill — e.g. on scroll, on wider viewports, or when a screen needs more header-level controls than the pill can hold. Needs a transition between the two states and a decision on what triggers it.
- [ ] **Custom Board Columns per project** — Let each project define its own set of Kanban columns, with per-project custom statuses *replacing* the fixed global `TaskStatus` enum entirely (not layered on top of it). Significant scope: a schema/migration decision for how existing tasks' statuses map onto the new per-project model, changes everywhere `TaskStatus` is referenced today (backend CQRS handlers/validators, `TransitionTask`, the shared DTO/enum in `@taskmanager/shared`, and the DS03 Kanban board column rendering), plus new UI for creating/reordering/deleting columns per project.
- [ ] **Project Settings page (per-project, admin-scoped)** — Dedicated settings screen for an individual project, reachable from within the project itself (mirroring PM02's "Members" panel placement — not the global admin panel), housing project-level admin controls that currently have no real home. First occupant: `Project.CriticalityScore` (1–10, admin-settable — see PRI01.4's `PUT /api/projects/{id}/criticality-score` endpoint). **Interim home, revised after user direction:** rather than a per-project panel control, **PRI02.5** shipped this as a standalone admin-section screen instead — `frontend/packages/web/src/screens/admin/ProjectCriticalityScreen/` (wired into `AdminShell.tsx`'s nav as "Project Criticality"), listing every project with an inline editable score (a Save button per row, added after an initial commit-on-blur version turned out not to feel reliable). See [CompletedTickets.md](CompletedTickets.md#pri02-tickets--frontend-voting-ui). When this page is built, PRI02.5's screen (or just its per-project control) should be retired/migrated into the real per-project Settings page described above. Related but distinct from the other "no real settings system exists yet" gaps already tracked in this section: the font and self-service-profile items below are both *user*-scoped settings, not project-scoped; "Configurable card/task layout" below raises the same per-project-vs-global question this page would need to answer for any future per-project setting, and could become a second occupant of this page if a per-project answer is chosen for it. Not yet ticketed.
- [ ] **User-selectable system font** — Let each user pick their UI font from a curated set, persisted as a stored user setting (not just `localStorage`, unlike the current dark/light toggle — see [CompletedTickets.md](CompletedTickets.md#ds01-tickets--design-system-foundation) — implies this belongs alongside a real per-user Settings concept, which doesn't exist yet on either the backend `AppUser` or the frontend). Options: Outfit (system default), Geist (Sans / Mono / Pixel), Syne (Regular & Italic), Space Grotesk. Needs: font asset/self-hosting or CDN decision, a `fontFamily` (+ possibly `fontVariant` for the Geist/Syne sub-options) field on the stored user settings model, a Settings UI to select it, and a CSS token/variable swap mechanism (mirroring how `.light` swaps color tokens) applied at the app-shell level.
- [ ] **Global "All Projects" Board + full Sort/Filter controls** — The nav bar's "Board" tab is currently inert without a project already selected — `handleTabChange` in `App.tsx` has no case for it, so clicking it does nothing (it only becomes the active tab as a side effect of selecting a project via the Projects screen). Should become a real navigation target: clicking it with no project selected takes the user to a cross-project board showing all tasks across every project they can see (i.e. every project they're a `ProjectMember` of, per Phase 6), respecting the last-used filter state (or unfiltered by default). Coupled to filtering by necessity — a many-hundreds-of-tasks global view needs real filter controls (project, status, priority, assignee, weighted score) to be usable at all. Needs: a new API capability to fetch tasks across multiple/all-member projects (today's `GET tasks` is single-project only), `handleTabChange` gaining a real `'board'` case, a persisted "last filter state" mechanism (ties into the same open per-user-settings question flagged by the font backlog item above), and `KanbanBoard`/`ListView` updates to render mixed-project task sets (e.g. a per-card project indicator). Supersedes/absorbs the standalone "sorting & filtering controls" follow-up noted in PRI03 — see [CompletedTickets.md](CompletedTickets.md#pri03-tickets--default-sorting).
- [ ] **Configurable card/task layout** — Let admins redesign which fields display on `TaskCard`/`ListView` rows/`TodoList` rows and the task detail panel (priority, due date, assignee, epic, phase, weighted score, etc.), instead of today's fixed layout. **Open question, not yet decided:** should this be a single **global** admin setting (one layout for the whole instance), or a **per-project** setting (each project's board looks different)? The two have real, different implications — global is a simple system-settings row; per-project needs a `Project`-scoped config field/entity, and would need to coexist with the same per-project customization angle already flagged by "Custom Board Columns per project" above. Also needs a decision on granularity — free-form per-field toggles vs. a handful of preset layout templates. Ties into the same recurring "no real settings system exists yet" gap as the font and self-service-profile backlog items.
