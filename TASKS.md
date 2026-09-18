# Task Plan — HomeAssistant Project Tracker

> Epic / Feature board. Each epic's tickets are tracked in [TICKETS.md](TICKETS.md).
> Updated at the end of each session.

---

## 🏃 Sprint 1 — MVP (WebUI)

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 1 | [U01 — Build Kanban board & list dashboard](#u01) | ✅ Done | — |
| 2 | [ARCH01 — npm workspace monorepo restructure](#arch01) | ✅ Done | #1 |
| 3 | [BUG-04 — Fix API base URL / create .env.development](#bug-04) | ✅ Done | — |
| 4 | [BUG-01 — Add JsonStringEnumConverter to backend](#bug-01) | ✅ Done | — |
| 5 | [BUG-05 — Show errors in CreateProjectModal](#bug-05) | ✅ Done | — |
| 6 | [BUG-06 — Show API load errors in screens](#bug-06) | ✅ Done | — |

---

## 🎨 Sprint 1b — UI Visual Overhaul & Board Build

> Blocks all future UI work. Design system must be locked before building new screens.

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 7  | [DS01 — Design system: tokens, Tailwind config, CSS vars](#ds01) | ✅ Done | — |
| 8  | [DS02 — Restyle app shell & ProjectSelectorScreen](#ds02) | ✅ Done | #7 |
| 9  | [DS03 — Build Board screen (Kanban view)](#ds03) | ✅ Done | #7 |
| 10 | [DS04 — Task creation modal (web, inline — not FAB)](#ds04) | ✅ Done | — |
| 11 | [DS05 — Task detail panel](#ds05) | ✅ Done | — |
| 25 | [DS06 — Restyle List view in the new design system](#ds06) | ✅ Done | #11 |

---

## 🖥 Sprint 2 — Admin UI

> A03 and B05b are pushed post-MVP — see [Post-MVP Backlog](#-post-mvp-backlog). A01/A02 are both done.

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 3 | [A01 — Build desktop admin panel shell](#a01) | ✅ Done | — |
| 4 | [A02 — Build user management portal](#a02) | ✅ Done | #3 |

---

## 🧬 Sprint 3 — Data Model: Epic Entity

> All of Sprint 3 is done (DM01/DM02 signed off 2026-08-26, DM03 signed off 2026-08-26) — see [EPIC-ENTITY-PLAN.md](EPIC-ENTITY-PLAN.md) for the researched, approved implementation plan that kicked this off (note: its frontend file paths were corrected during ticketing — written before DS05/DS06 shipped). Expanded mid-planning to also cover DM03 — DailyTask projects becoming a simplified todo list — since both touch `ProjectScope`/project-type behavior. Tickets carry Fibonacci story points (capped at 5).

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 12 | [DM01 — Backend: Epic entity](#dm01) | ✅ Done | — |
| 13 | [DM02 — Frontend: Epic entity consumption](#dm02) | ✅ Done | #12 |
| 26 | [DM03 — DailyTask simplified todo list](#dm03) | ✅ Done | — |

---

## 🔐 Sprint 4 — Identity & Access (MVP-Required)

> New MVP-required sprint. The app has zero authentication today and no project-membership concept — both are prerequisites for Sprint 5's weighted voting. See [ROADMAP.md](ROADMAP.md#phase-6-identity--access-mvp-required) for the full rationale.

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 17 | [AUTH01 — Backend: Authentication & API tokens](#auth01) | ✅ Done | — |
| 18 | [AUTH02 — Frontend: Login & session](#auth02) | ✅ Done | #17 |
| 20 | [PM01 — Backend: Project membership](#pm01) | ✅ Done | #17 |
| 21 | [PM02 — Frontend: Project membership UI](#pm02) | ✅ Done | #20, #18 |

---

## ⚖️ Sprint 5 — Weighted Priority Scoring (MVP-Required)

> New MVP-required sprint. Depends on Sprint 4 (needs a real logged-in identity to attribute votes to, and `ProjectMember` to know who's eligible to vote). See [ROADMAP.md](ROADMAP.md#phase-7-weighted-priority-scoring-mvp-required).

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 22 | [PRI01 — Backend: Voting & weighted scoring](#pri01) | ✅ Done | Sprint 4 |
| 23 | [PRI02 — Frontend: Voting UI](#pri02) | ✅ Done | #22 |
| 24 | [PRI03 — Default sorting by weighted score](#pri03) | ✅ Done | #22, #23 |

---

## 🐛 Sprint 6 — Bug Fixes & Deployment Prep

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 7 | [BUGS — Resolve known bug tickets](#bugs) | ✅ Done | — |
| 8 | [PREP — Production readiness & environment config](#prep) | ✅ Done | #7 |

---

## 🚀 Sprint 7 — Initial Deployment (Monolithic Production Image) *(all ✅ Done, archived)*

> Redefined per user direction (2026-08-30): rather than three separate images orchestrated via docker-compose, production is a single monolithic container — Postgres, the API, and the built frontend all run together under one process supervisor (s6-overlay). See [ROADMAP.md](ROADMAP.md#phase-9-containerization--initial-deployment). All three epics signed off 2026-08-31 — see [CompletedTickets.md](CompletedTickets.md#d01d03-tickets--monolithic-production-docker-image).

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 9  | [D01 — Backend & frontend build stages](#d01d03) | ✅ Done | #8 |
| 10 | [D02 — Monolithic runtime base (Postgres + Nginx + supervisor)](#d01d03) | ✅ Done | #8 |
| 11 | [D03 — Assemble, wire & verify the production image](#d01d03) | ✅ Done | #9, #10 |

---

## ✨ Sprint 8 — Post-Deployment: Quick Capture

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 14 | [U02 — Implement FAB quick-capture modal](#u02) | 🔲 Pending | #11 |

---

## 🤖 Sprint 9 — Post-Deployment: MCP Agent Integration

> Redesigned per user direction (2026-09-08) — supersedes the original M01–M04 sketch (Node.js/TypeScript, REST-only, Projects+Tasks scope), which predated Epic existing as a real entity (Sprint 3) and predated the `ApiToken` auth scheme (Sprint 4). `TaskManager.Mcp` is now C#, talks to `TaskManager.API` over gRPC (internal-only), and exposes itself to AI clients over remote Streamable HTTP. See [ROADMAP.md](ROADMAP.md#phase-11-post-deployment--mcp-server-agent-integration-layer) for the phase writeup and [TICKETS.md](TICKETS.md#mcp01-tickets--grpc-contracts--api-grpc-surface) for tickets.

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 18 | [MCP01 — gRPC Contracts + API gRPC Surface](#mcp01) | ✅ Done | #11 |
| 19 | [MCP02 — ApiToken Scoping](#mcp02) | 🔲 Pending | #11 |
| 20 | [MCP03 — Scaffold TaskManager.Mcp](#mcp03) | 🔲 Pending | #18 |
| 21 | [MCP04 — Hierarchy MCP Tools](#mcp04) | 🔲 Pending | #20, #19 |
| 22 | [MCP05 — Agent Plan Tracking MCP Tools](#mcp05) | 🔲 Pending | #20 |
| 23 | [MCP06 — Frontend: MCP Tokens Admin Screen](#mcp06) | 🔲 Pending | #19 |
| 24 | [MCP07 — Containerize & Wire into docker-compose](#mcp07) | 🔲 Pending | #20, #21, #22 |

---

## 🐘 Sprint 10 — Post-Deployment: Disable Bundled Postgres Toggle

> Graduated out of the [Post-MVP Backlog](#-post-mvp-backlog) per user direction (2026-09-01) — being picked up now, not deferred further. Extends D01–D03's monolithic production image; see [CompletedTickets.md](CompletedTickets.md#d01d03-tickets--monolithic-production-docker-image) for the image this builds on.

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 17 | [D04 — Disable Bundled Postgres Toggle](#d04) | 🔲 Pending | #11 |

---

## 📋 Epic Descriptions

### U01
**Build Kanban board & list dashboard** — ✅ Done
Two view modes (Kanban columns + prioritized list), data hooks, shared task card components, view toggle persisted to localStorage. See [CompletedTickets.md](CompletedTickets.md#u01-tickets) for the full ticket history.

---

### BUG-04
**Fix API base URL / create .env.development** — ✅ Done
`ApiClient.ts` fell back to the wrong port with no `.env.development` file, breaking every dev-mode API call. Fixed by adding `packages/web/.env.development` (and a documented `.env.example`) pointing at the correct port. See [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking) for the full ticket history.

---

### BUG-01
**Add JsonStringEnumConverter to backend** — ✅ Done
`System.Text.Json` rejected the frontend's string enum values by default, breaking every write endpoint. Fixed by chaining `.AddJsonOptions()` with `JsonStringEnumConverter` onto `AddControllers()` in `Program.cs`. See [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking) for the full ticket history.

---

### BUG-05
**Show errors in CreateProjectModal** — ✅ Done
`handleSubmit` silently swallowed API errors with no `catch` block. Added error state that renders inline on failure and clears on retry, verified across happy/failure/retry paths. See [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking) for the full ticket history.

---

### BUG-06
**Show API load errors in screens** — ✅ Done
`useProjects`/`useTasks` returned an `error` field that no screen rendered, so failed loads looked identical to empty data. Both `ProjectSelectorScreen` and `BoardScreen` now show a "Failed to load" message when `error` is non-null. See [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking) for the full ticket history.

---

### ARCH01
**npm workspace monorepo restructure** — ✅ Done
Replaced the single Vite app with an npm workspaces monorepo (`@taskmanager/shared`, `/ui`, `/web`, `/mobile`), resolving a dev-environment WASM crash and separating web-first from mobile-first concerns. See [CompletedTickets.md](CompletedTickets.md#arch01-tickets) for the full ticket history.

---

### DS01
**Design system: tokens, Tailwind config, CSS vars** — ✅ Done
Established the zinc/crimson palette and dark/light mode split as a Tailwind v4 `@theme` block + `.light` class override (no `tailwind.config.ts`, since the project runs CSS-first Tailwind v4). All subsequent components consume these tokens instead of raw Tailwind color classes. See [CompletedTickets.md](CompletedTickets.md#ds01-tickets--design-system-foundation) for the full ticket history.

---

### DS02
**Restyle app shell & ProjectSelectorScreen** — ✅ Done
Applied the design system to the app shell (which pivoted from a conventional header bar to a chrome-less floating-pill shell — see [MISSION.md](MISSION.md#-design--style-decisions)) and to the project selector's cards and Create Project modal, verified in both dark and light mode. See [CompletedTickets.md](CompletedTickets.md#ds02-tickets--app-shell--projectselectorscreen-restyle) for the full ticket history including follow-up rounds.

---

### DS03
**Build Board screen (Kanban view)** — ✅ Done
Built the Kanban board from scratch with native HTML5 drag-and-drop status transitions (a `@dnd-kit/core` migration was considered and deferred as **MOB04**, post-MVP), and fixed BUG-07 (locking out invalid drop targets for Done/Cancelled tasks) plus added a failure toast. See [CompletedTickets.md](CompletedTickets.md#ds03-tickets) for the full ticket history.

---

### DS04
**Task creation modal (web, inline — not FAB)** — ✅ Done
A reusable `Modal` shell hosts `TaskForm` (title, priority, due date, assignee), wired into `BoardScreen` via a "+ New Task" button that creates the task and refetches the board. See [CompletedTickets.md](CompletedTickets.md#ds04-tickets) for the full ticket history.

---

### DS05
**Task detail panel** — ✅ Done
Slide-in panel for viewing/editing a task, reworked mid-epic from always-editable to a read-only default with explicit Edit/Save/Cancel per direct user feedback; also fixes BUG-02 (stale phase highlight) and BUG-03 (stale list rows after edits). See [CompletedTickets.md](CompletedTickets.md#ds05-tickets) for the full ticket history.

---

### DS06
**Restyle List view in the new design system** — ✅ Done
Restyled `ListView`'s rows/chrome onto design tokens; the planned panel-consolidation ticket (DS06.2) turned out to be a no-op, since DS05 had already given Kanban and List view one shared `TaskDetailPanel` instance. See [CompletedTickets.md](CompletedTickets.md#ds06-tickets--restyle-list-view) for the full ticket history.

---

### A01
**Build desktop admin panel shell** — ✅ Done
Desktop-only layout with side navigation; `AdminGuard.tsx` locks out sub-1024px viewports live (not just on mount), and the admin entry point in `FloatingPill` is fully absent for non-admins. See [CompletedTickets.md](CompletedTickets.md#a01-tickets--admin-panel-shell--guardrails) for the full ticket history (A01.1–A01.3, 10 points).

---

### A02
**Build user management portal** — ✅ Done
Added the missing status/role/avatar backend endpoints (`Deactivate`/`Reactivate`/`PromoteToAdmin`/`RevokeAdmin`/avatar upload) plus a frontend user list, create-user modal, and row actions; fixed two real bugs found during testing (a raw 500 on promoting a second admin, and avatars not rendering anywhere due to an origin-mismatch bug). See [CompletedTickets.md](CompletedTickets.md#a02-tickets--user-management-portal) for the full ticket history (A02.1–A02.6, 17 points).

---

### A03
**Build webhook & integration manager** — ⏸ Deferred (Post-MVP)
Admin screen: webhook URL config, show inbound webhook URLs and project mappings, execution log of last 10 payloads. Requires B05b (also deferred post-MVP — see below), so this couldn't have shipped pre-MVP regardless. API access token generation/revocation lives in AUTH03 instead (also deferred) — this screen consumes that backend. Not yet broken into tickets.

---

### B05b
**Implement integration gateway (IntegrationController)** — ⏸ Deferred (Post-MVP)
`IntegrationController` with API-token auth middleware (reuses the `ApiToken` infrastructure built in Sprint 4's AUTH01, not a separate scheme), POST endpoint for inbound payloads from Home Assistant or scripts, payload parsing that creates/updates tasks via Application layer. Already flagged post-MVP in [ROADMAP.md](ROADMAP.md) Phase 2 — moved out of the active Sprint 2 table to match.

---

### BUGS
**Resolve known bug tickets** — ✅ Done
All seven bug tickets (BUG-01–07) are resolved, with no open bugs remaining. See [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking) and [CompletedTickets.md](CompletedTickets.md#sprint-3-bug-tickets--ux-polish-pre-deployment) for the full ticket history.

---

### PREP
**Production readiness & environment config** — ✅ Done (signed off 2026-08-30)
Hardened the app for production: a DB-backed, admin-editable CORS allow-list; a `/health` endpoint; confirmed env-var wiring for the production build and the connection string/JWT secret (with a Production startup guard); confirmed Serilog sinks; and removal of the legacy pre-monorepo `frontend/src/` tree. Closes out Sprint 6 / [ROADMAP.md](ROADMAP.md#phase-8-bug-fixes--deployment-prep) Phase 8. See [CompletedTickets.md](CompletedTickets.md#prep-tickets--production-readiness--environment-config) for the full ticket history (PREP.1–PREP.6, 12 points).

---

### DM01
**Backend: Epic entity** — ✅ Done (signed off 2026-08-26)
New `Epic` domain entity as a sibling of `ProjectPhase`, with full CQRS CRUD, an optional `EpicId` FK on `ProjectTask`, and removal of `Epic` from the `ProjectScope` enum. Full researched plan: [EPIC-ENTITY-PLAN.md](EPIC-ENTITY-PLAN.md). See [CompletedTickets.md](CompletedTickets.md#dm01-tickets--backend-epic-entity) for the full ticket history (DM01.1–DM01.4, 16 points).

---

### DM02
**Frontend: Epic entity consumption** — ✅ Done (signed off 2026-08-26)
Shared types, a `useEpics` hook, and a scope-gated Epic picker in `TaskDetailPanel`; expanded twice post-ship with a full Epics management panel and randomly-assigned, per-project-excluded badge colors on both Kanban and List views. See [CompletedTickets.md](CompletedTickets.md#dm02-tickets--frontend-epic-entity-consumption) for the full ticket history (DM02.1–DM02.7, 12+ points).

---

### DM03
**DailyTask projects become a simplified todo list** — ✅ Done (signed off 2026-08-26)
DailyTask-scope projects now show only a list view with simple complete/incomplete tasks (reusing the existing `TaskStatus` enum) and no Phases or Epics, enforced at the UI layer only. Closes out Sprint 3 in full. See [CompletedTickets.md](CompletedTickets.md#dm03-tickets--dailytask-simplified-todo-list) for the full ticket history (DM03.1–DM03.3, 9 points).

---

### AUTH01
**Backend: Authentication & API tokens** — ✅ Done (Sprint 4, MVP-Required)
JWT login with a global `[Authorize]` fallback policy, plus an `ApiToken` scheme for non-human/service accounts authenticated via `X-Api-Token`. See [CompletedTickets.md](CompletedTickets.md#auth01-tickets--backend-authentication--api-tokens) for the full ticket history (AUTH01.1–AUTH01.5).

---

### AUTH02
**Frontend: Login & session** — ✅ Done (Sprint 4, MVP-Required)
`LoginScreen` plus an axios interceptor that attaches the bearer token and auto-clears the session on 401; expanded mid-sprint with a one-time `POST /api/auth/setup` and `SetupScreen` to bootstrap the very first admin account. See [CompletedTickets.md](CompletedTickets.md#auth02-tickets--frontend-login--session) for the full ticket history (AUTH02.1–AUTH02.4).

---

### AUTH03
**Admin: API token management UI** — ⏸ Deferred (Post-MVP)
Admin screen to generate/revoke `ApiToken`s (from AUTH01). Token value is shown once at creation with a copy-to-clipboard action and never displayed again. Absorbs the token-management half of A03 — A03 becomes this backend's UI consumer rather than duplicating it. Pushed post-MVP per user direction — AUTH01's backend already works standalone via direct API calls, so this is UI-only remaining scope. See [TICKETS.md](TICKETS.md#auth03-tickets--admin-api-token-management-ui) for the still-specced ticket, ready to pick up post-deployment.

---

### PM01
**Backend: Project membership** — MVP-Required (Sprint 4) — ✅ Done (signed off 2026-08-24)
New `ProjectMember` join entity with flat membership (no per-project roles), auto-assigning a project's creator as its first member, plus CQRS + controller for add/remove/list; found and fixed a real EF Core bug where the `Guid`-key convention misread a new member row as an update. See [CompletedTickets.md](CompletedTickets.md#pm01-tickets--backend-project-membership) for the full ticket history (PM01.1–PM01.3).

---

### PM02
**Frontend: Project membership UI** — MVP-Required (Sprint 4) — ✅ Done (signed off 2026-08-25)
A "Members" panel reachable from within the project (not the admin panel) for adding/removing members; expanded ad-hoc (PM02.3) to restrict task assignment to actual project members on both frontend and backend. See [CompletedTickets.md](CompletedTickets.md#pm02-tickets--frontend-project-membership-ui) for the full ticket history (PM02.1–PM02.3).

---

### PRI01
**Backend: Voting & weighted scoring** — MVP-Required (Sprint 5) — ✅ Done (signed off 2026-08-28)
New `TaskVote` entity and `Project.CriticalityScore`, with weighted score computed as `ceiling((PriorityNumeric + AvgUserVote + CriticalityScore) / 3)`, applied uniformly across all project scopes including DailyTask. See [CompletedTickets.md](CompletedTickets.md#pri01-tickets--backend-voting--weighted-scoring) for the full ticket history (PRI01.1–PRI01.4, 11 points).

---

### PRI02
**Frontend: Voting UI** — MVP-Required (Sprint 5) — ✅ Done (signed off 2026-08-28)
1–10 voting control in the shared `TaskDetailPanel` plus weighted score display on `TaskCard`/`ListView`/`TodoList`; expanded ad-hoc (PRI02.5) with an interim admin screen for setting a project's `CriticalityScore`, since no ticket had built a frontend control for PRI01.4's endpoint. See [CompletedTickets.md](CompletedTickets.md#pri02-tickets--frontend-voting-ui) for the full ticket history (PRI02.1–PRI02.5, 14 points).

---

### PRI03
**Default sorting by weighted score** — MVP-Required (Sprint 5) — ✅ Done (signed off 2026-08-28)
Board/List/`TodoList` views and the project selector now default-sort by weighted score / `CriticalityScore` descending; full sort/filter controls are logged as a post-MVP backlog item. See [CompletedTickets.md](CompletedTickets.md#pri03-tickets--default-sorting) for the full ticket history (PRI03.1–PRI03.2, 4 points).

---

### U02
**Implement FAB quick-capture modal** — Post-deployment
Floating Action Button on the board screen that opens a bottom sheet modal. Supports task title, project selector, priority picker, single-tap assignee, and optional due date. On submit, calls `createTask()` from `@taskmanager/shared` directly and closes instantly — the board refetches. See [TICKETS.md](TICKETS.md#u02-tickets) for tickets.

---

### MCP01
**gRPC Contracts + API gRPC Surface** — ✅ Done (signed off 2026-09-17)
New `TaskManager.Grpc.Contracts` project (`.proto` files mirroring the existing DTOs field-for-field) plus `ProjectsGrpcService`/`EpicsGrpcService`/`TasksGrpcService`/`PhasesGrpcService` on `TaskManager.API`, all calling the same MediatR commands/queries the REST controllers already call over a second, internal-only Kestrel HTTP/2 port — no new business logic, no REST controllers touched. Verified fully live against the real dev DB across all four tickets (auth, data-parity with REST, validation/transition error mapping). See [CompletedTickets.md](CompletedTickets.md#mcp01-tickets--grpc-contracts--api-grpc-surface) for the full ticket history (MCP01.1–MCP01.4).

---

### MCP02
**ApiToken Scoping** — 🔲 Pending
`ApiToken` gains `IsReadOnly` (default `false`), `ExpiresAt` (default `null`), `RateLimitPerMinute` (default `null` = system default, never unlimited), and a computed `IsExpired` property, plus an EF migration (`ApiTokenScoping`). `ApiTokenAuthenticationHandler` rejects expired tokens and adds `isReadOnly`/`rateLimitPerMinute` claims. New write-guard middleware (not an MVC action filter — action filters don't run for gRPC endpoints) blocks non-safe requests from read-only tokens, covering REST and gRPC uniformly. New in-process rate-limiting middleware (`Microsoft.AspNetCore.RateLimiting`, partitioned by `apiTokenId` claim, no Redis/external limiter — single-host deployment) covers both transports the same way. `CreateApiTokenCommand`/`ApiTokensController` gain the three new fields; stays `[Authorize(Roles = "Admin")]`. See [TICKETS.md](TICKETS.md#mcp02-tickets--apitoken-scoping) for tickets.

---

### MCP03
**Scaffold TaskManager.Mcp** — 🔲 Pending
New standalone `backend/TaskManager.Mcp/` project — deliberately not layered into Domain/Application/Infrastructure/API like the monolith; two entities and a handful of tools don't earn that structure. `Program.cs` wires `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()` + `app.MapMcp()`, configured via `TASKMANAGER_GRPC_URL` and `MCP_DB_CONNECTION_STRING`. `McpDbContext` (EF Core) targets a new `McpTracking` Postgres database — same Postgres instance as the monolith, separate database, separate migration history, no FK coupling to `TaskManager.Domain` — hosting new `AgentPlan`/`AgentStep` entities (`AgentPlanStatus`/`AgentStepStatus` enums, agent ID/session ID metadata, `AgentPlan` scoped to a required `ProjectId`) plus an initial migration. A `GrpcClients/` wrapper around the generated `TaskManager.Grpc.Contracts` stubs, forwarding the inbound MCP bearer token as gRPC call credentials on every downstream call. See [TICKETS.md](TICKETS.md#mcp03-tickets--scaffold-taskmanagermcp) for tickets.

---

### MCP04
**Hierarchy MCP Tools** — 🔲 Pending
`list_projects`, `get_project`, `list_epics`, `create_epic`, `update_epic`, `list_phases`, `list_tasks`, `get_task`, `create_task`, `update_task`, `transition_task` — each forwards the incoming MCP request's bearer token as gRPC call credentials on the outbound `GrpcClients/` call. No auth logic of its own: a bad/expired/revoked token surfaces as an `UNAUTHENTICATED` gRPC status mapped to an MCP-level auth error; a mutating tool called with a read-only token surfaces the API's `PERMISSION_DENIED` cleanly, no client-side scope enforcement duplicated here. See [TICKETS.md](TICKETS.md#mcp04-tickets--hierarchy-mcp-tools) for tickets.

---

### MCP05
**Agent Plan Tracking MCP Tools** — 🔲 Pending
`create_agent_plan`, `get_agent_plan`, `add_agent_step`, `complete_agent_step`, `fail_agent_step`, `complete_agent_plan` — read/write `McpDbContext` directly, no network hop, no dependency on `TaskManager.API` for this data. An additive audit trail of what an agent actually did, on top of the human-visible tickets it creates via MCP04's hierarchy tools — not shown on the Kanban/List board by default, not required to satisfy the original "AI creates human-visible tickets" ask. See [TICKETS.md](TICKETS.md#mcp05-tickets--agent-plan-tracking-mcp-tools) for tickets.

---

### MCP06
**Frontend: MCP Tokens Admin Screen** — 🔲 Pending
New `McpTokensScreen` (mirrors `AllowedOriginsScreen`'s shape: table + add form + per-row revoke), extended for MCP02's new fields — name, read-only toggle, expiry date picker, rate limit input, created date, revoke button. Raw token shown once at creation, never retrievable again. `AdminShell.tsx` gains an `'mcpTokens'` screen + nav entry, same pattern as the existing admin screens. `packages/shared/src/ApiClient.ts`'s `ApiTokenDto`/`CreateApiTokenRequest` gain the new fields. **Overlaps in purpose with the still-deferred [AUTH03](TICKETS.md#auth03-tickets--admin-api-token-management-ui)** (a generic, pre-scoping token-management screen) — flagged for the user to decide whether AUTH03 should be retired in favor of this screen once MCP06 ships; not resolved automatically here. See [TICKETS.md](TICKETS.md#mcp06-tickets--frontend-mcp-tokens-admin-screen) for tickets.

---

### MCP07
**Containerize & Wire into docker-compose** — 🔲 Pending
`TaskManager.Mcp` Dockerfile (multi-stage .NET build) and a new `docker-compose.yml` service entry. The MCP HTTP endpoint is exposed over Tailscale/LAN; the gRPC leg to `TaskManager.API` and the `McpTracking` Postgres connection stay internal-only on the Docker network, same deployment posture as the rest of the stack (mirrors D01–D03's monolithic image + D04's external-Postgres toggle). See [TICKETS.md](TICKETS.md#mcp07-tickets--containerize--wire-into-docker-compose) for tickets.

---

### D01–D03
**Monolithic production Docker image (build stages, runtime base, assembly & verification)** — ✅ Done (signed off 2026-08-31)
A single root-level `Dockerfile` builds and runs Postgres, the .NET API, and the built frontend together under s6-overlay on Alpine, with Nginx proxying `/api`/`/avatars` to the API and the API also directly exposed for future consumers (MCP, webhooks). Verified live end-to-end (build, boot order, first-run setup, direct vs. proxied API access, data persistence across container recreation); closes out Sprint 7 / [ROADMAP.md](ROADMAP.md#phase-9-containerization--initial-deployment) Phase 9. See [CompletedTickets.md](CompletedTickets.md#d01d03-tickets--monolithic-production-docker-image) for the full ticket history (D01.1–D01.2, D02.1–D02.2, D03.1–D03.2).

---

### D04
**Disable Bundled Postgres Toggle** — 🔲 Pending
Adds a `DISABLE_BUNDLED_POSTGRES` env var to the D01–D03 monolithic production image, letting deployments that always point at an external Postgres (via `DatabaseSettings__ConnectionString`) skip starting the bundled instance entirely, instead of it always initializing and running unused. Touches all three s6-overlay processes: `postgres-init` (oneshot) exits immediately without `initdb`/`createdb`; `postgres` (longrun) execs `pause` instead of Postgres so s6-rc sees a stable no-op process rather than a crash loop; `api`'s startup script skips its `pg_isready` wait loop, which today always polls the local bundled instance regardless of where the configured connection string actually points. Graduated out of the Post-MVP Backlog per user direction (2026-09-01) — see [TICKETS.md](TICKETS.md#d04-tickets--disable-bundled-postgres-toggle) for tickets.

**D04.1 implementation status (2026-09-01): implementation complete, awaiting user sign-off** — epic status stays 🔲 Pending until confirmed. This turned out to be a real bug the user had hit in practice, not just the backlog item's original "resource waste" framing: pointing the container at an external Postgres left the API endlessly logging "waiting for postgres" while the external Postgres showed zero incoming connections, only starting "after a few restarts." Root cause was `api-run.sh` polling `pg_isready` against the bundled local Postgres unconditionally, regardless of what `DatabaseSettings__ConnectionString` actually pointed at. Implemented as ticketed across `postgres-init.sh`, `postgres-run.sh`, and `api-run.sh`, with one correction found during verification — the actual s6-overlay pause utility is `s6-pause`, not `pause` as the ticket's background text said. Verified live with real containers (not simulated) against both a real throwaway external Postgres and a regression pass with the toggle unset — no regression found. Full detail in [TICKETS.md](TICKETS.md#d04-tickets--disable-bundled-postgres-toggle) under D04.1.

---

## 📌 Status Key

| Symbol | Meaning |
|---|---|
| 🔲 Pending | Not started |
| 🔄 In Progress | Actively being built |
| 🔍 In Review | Implemented + verified, awaiting user sign-off |
| ✅ Done | Complete |
| ⏸ Deferred | Moved to post-MVP |

---

## 🗂 Post-MVP Backlog

### Mobile App Phase
- **U03** — Gesture-driven task interactions (swipe-to-complete, swipe-to-assign) in `packages/mobile`
- **MOB01** — Wire `packages/mobile` Capacitor build for iOS/Android
- **MOB02** — Offline-first sync: full mutation queue replay, conflict resolution
- **MOB03** — Push notifications for task assignments (Capacitor Push)

### Platform Extensions
- **AUTH03** — Admin: API Token Management UI. Pushed post-MVP per user direction; AUTH01's backend (generate/list/revoke `ApiToken`s) already ships and works via direct API calls, so this is UI-only remaining scope. Ticket fully specced and ready to pick up — see [TICKETS.md](TICKETS.md#auth03-tickets--admin-api-token-management-ui).
- **Admin: Transfer sole-admin status** — A02.1 confirmed (per user direction, following AUTH02.3's DB-level constraint) that the app enforces exactly one admin, always. But A02's promote/revoke endpoints are independent toggles: promote is blocked once an admin exists, and revoke is blocked when it's the last one — so there is currently no way to ever change who that one admin is. Needs an atomic "transfer admin" operation (revoke + promote in a single transaction) exposed via the admin UI. Not yet ticketed.
- **A03** — Admin: Webhook & Integration Manager. Pushed post-MVP per user direction; also genuinely blocked by B05b (also post-MVP, see below) regardless. Not yet ticketed.
- **Admin: Edit user profiles + self-service profile editing** — Admin can edit another user's display name/email and reset their password (extends A02's User Management Portal). Separately, every user gets a "My Account" screen to edit their own name/email/password — no self-service profile editing exists anywhere today. Needs backend endpoints that distinguish admin-on-behalf-of edits from self-service ones (self-directed password changes require the current password; admin resets don't), plus a new "My Account" screen (same missing per-user Settings concept as the font backlog item). See [ROADMAP.md](ROADMAP.md#post-mvp-backlog) for the full writeup. Not yet ticketed.
- **Project Settings page (per-project, admin-scoped)** — Real settings surface for individual projects. First need: `Project.CriticalityScore` only has an **interim** admin screen (`ProjectCriticalityScreen`, PRI02.5) explicitly built as a stopgap pending this page. Likely also ends up housing Configurable card/task layout (if it resolves per-project) and a per-project home for Custom Board Columns. See [ROADMAP.md](ROADMAP.md#post-mvp-backlog) for the full writeup. Not yet ticketed.
- **B05b** — Integration Gateway (`IntegrationController`). Already flagged post-MVP in [ROADMAP.md](ROADMAP.md) Phase 2; moved out of the active Sprint 2 table in TASKS.md to match. A03's hard dependency.
- **RBAC** — Fine-grained role-based access control beyond Sprint 4's flat `ProjectMember` + global `AppUser.IsAdmin` model. Per-project roles (owner/contributor/viewer) with distinct permissions. Deferred per user direction.
- **Global "All Projects" Board + full Sort/Filter controls** — The nav bar's "Board" tab is currently inert without a project already selected (`handleTabChange` in `App.tsx` has no case for it). Should become a real navigation target: a cross-project board showing all tasks across every project the user can see (per Phase 6's `ProjectMember`), respecting the last-used filter state. Coupled to full sort/filter controls (project, status, priority, assignee, weighted score) by necessity — beyond PRI03's default-order-only sort. Needs a new multi-project task-fetch API, a persisted "last filter state" mechanism, and mixed-project rendering support in `KanbanBoard`/`ListView`. See [ROADMAP.md](ROADMAP.md#post-mvp-backlog) for the full writeup.
- **Configurable card/task layout** — Let admins choose which fields display on `TaskCard`/`ListView`/`TodoList` rows and the task detail panel, instead of today's fixed layout. Open question, not yet decided: global admin setting vs. per-project setting — the two have real, different implementation paths. Ties into the same "no real settings system exists yet" gap as the font and self-service-profile items. See [ROADMAP.md](ROADMAP.md#post-mvp-backlog) for the full writeup.
- ~~**Docker: Disable Bundled Postgres**~~ — Graduated out of the backlog (2026-09-01), now ticketed as **D04** — see [TASKS.md](TASKS.md#d04) and [TICKETS.md](TICKETS.md#d04-tickets--disable-bundled-postgres-toggle).
- **B06** — Background Automation Engine (async webhook queue processing)
- **Home Assistant Integration** — Native HA integration beyond webhooks
- **Configurable Logging** — Runtime log level/sink settings in admin UI
- **Custom Webhook Builder** — Define outbound webhooks dynamically from admin UI
- **In-app "needs your vote" notifications** — Bell icon/badge surfacing tasks the current user hasn't yet voted on (Phase 7's voting system). In-app only, not push/email/SMS. Depends on Phase 7 shipping first. See [ROADMAP.md](ROADMAP.md#post-mvp-backlog) for the full writeup.
- **Email Notifications** — Assignee alerts on task create/update/complete
- **SMS Notifications** — High-priority task assignment alerts
- **BFF (Backend for Frontend)** — Per-client optimized API layer once web + mobile diverge meaningfully
