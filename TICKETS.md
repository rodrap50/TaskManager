# Tickets — HomeAssistant Project Tracker

> Active granular tickets for pending epics.
> Epics live in [TASKS.md](TASKS.md) · Completed tickets archived in [CompletedTickets.md](CompletedTickets.md).

---

## Table of Contents

**🐛 Bug Tickets** *(all ✅ Done, archived)*
- *Sprint 1 Bug Tickets — App-Breaking — BUG-01, BUG-04, BUG-05, BUG-06 — archived to [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking)*
- *Sprint 3 Bug Tickets — UX Polish (Pre-Deployment) — BUG-02, BUG-03, BUG-07 — archived to [CompletedTickets.md](CompletedTickets.md#sprint-3-bug-tickets--ux-polish-pre-deployment)*

**🎨 Sprint 1b — UI Visual Overhaul & Board Build** *(all ✅ Done, archived)*
- *DS01 — Design System Foundation — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#ds01-tickets--design-system-foundation)*
- *DS02 — App Shell & ProjectSelectorScreen Restyle — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#ds02-tickets--app-shell--projectselectorscreen-restyle)*
- *DS03 — Board Screen (Kanban View) — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#ds03-tickets)*
- *DS04 — Task Creation Modal (Web) — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#ds04-tickets)*
- *DS05 — Task Detail Panel — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#ds05-tickets)*
- *DS06 — Restyle List View — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#ds06-tickets)*

**🖥 Sprint 2 — Admin UI** *(all ✅ Done, archived)*
- *A01 — Build desktop admin panel shell — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#a01-tickets--admin-panel-shell--guardrails)*
- *A02 — Build user management portal — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#a02-tickets--user-management-portal)*

**🧬 Sprint 3 — Data Model: Epic Entity** *(all ✅ Done, archived)*
- *DM01 — Backend: Epic Entity — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#dm01-tickets--backend-epic-entity)*
- *DM02 — Frontend: Epic Entity Consumption — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#dm02-tickets--frontend-epic-entity-consumption)*
- *DM03 — DailyTask Simplified Todo List — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#dm03-tickets--dailytask-simplified-todo-list)*

**🔐 Sprint 4 — Identity & Access (MVP-Required)** *(all ✅ Done, archived)*
- *AUTH01 — Backend: Authentication & API Tokens — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#auth01-tickets--backend-authentication--api-tokens)*
- *AUTH02 — Frontend: Login & Session — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#auth02-tickets--frontend-login--session)*
- *PM01 — Backend: Project Membership — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#pm01-tickets--backend-project-membership)*
- *PM02 — Frontend: Project Membership UI — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#pm02-tickets--frontend-project-membership-ui)*

**⚖️ Sprint 5 — Weighted Priority Scoring (MVP-Required)** *(all ✅ Done, archived)*
- *PRI01 — Backend: Voting & Weighted Scoring — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#pri01-tickets--backend-voting--weighted-scoring)*
- *PRI02 — Frontend: Voting UI — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#pri02-tickets--frontend-voting-ui)*
- *PRI03 — Default Sorting — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#pri03-tickets--default-sorting)*

**🐛 Sprint 6 — Bug Fixes & Deployment Prep** *(all ✅ Done, archived)*
- *PREP — Production Readiness & Environment Config — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#prep-tickets--production-readiness--environment-config)*

**🚀 Sprint 7 — Initial Deployment (Monolithic Production Image)** *(all ✅ Done, archived)*
- *D01–D03 — Backend & Frontend Build Stages, Monolithic Runtime Base, Assemble/Wire/Verify — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#d01d03-tickets--monolithic-production-docker-image)*

**✨ Sprint 8 — Post-Deployment: Quick Capture**
- [U02 — FAB Quick-Capture Modal](#u02-tickets) — U02.1–U02.5

**🤖 Sprint 9 — Post-Deployment: MCP Agent Integration**
- *MCP01 — gRPC Contracts + API gRPC Surface — ✅ Done, archived to [CompletedTickets.md](CompletedTickets.md#mcp01-tickets--grpc-contracts--api-grpc-surface)*
- [MCP02 — ApiToken Scoping](#mcp02-tickets--apitoken-scoping) — MCP02.1–MCP02.5
- [MCP03 — Scaffold TaskManager.Mcp](#mcp03-tickets--scaffold-taskmanagermcp) — MCP03.1–MCP03.3
- [MCP04 — Hierarchy MCP Tools](#mcp04-tickets--hierarchy-mcp-tools) — MCP04.1–MCP04.2
- [MCP05 — Agent Plan Tracking MCP Tools](#mcp05-tickets--agent-plan-tracking-mcp-tools) — MCP05.1–MCP05.2
- [MCP06 — Frontend: MCP Tokens Admin Screen](#mcp06-tickets--frontend-mcp-tokens-admin-screen) — MCP06.1–MCP06.2
- [MCP07 — Containerize & Wire into docker-compose](#mcp07-tickets--containerize--wire-into-docker-compose) — MCP07.1–MCP07.3

**🐘 Sprint 10 — Post-Deployment: Disable Bundled Postgres Toggle**
- [D04 — Disable Bundled Postgres Toggle](#d04-tickets--disable-bundled-postgres-toggle) — D04.1–D04.2

**⏸ Deferred (Post-MVP)**
- [AUTH03 — Admin: API Token Management UI](#auth03-tickets--admin-api-token-management-ui) — AUTH03.1 *(pushed post-MVP; tokens remain manageable via direct API call in the meantime)*
- A03 — Build Webhook & Integration Manager *(pushed post-MVP; blocked by B05b, also post-MVP — not yet ticketed, see [TASKS.md](TASKS.md#a03))*
- [U03 — Gesture-Driven Interactions](#u03-tickets) *(Mobile App Phase)*

---

## Status Key

| Symbol | Meaning |
|---|---|
| 🔲 Pending | Not started |
| 🔄 In Progress | Actively being built |
| 🔍 In Review | Implemented + verified, awaiting user sign-off |
| ✅ Done | Complete |
| ⏸ Deferred | Moved to post-MVP |

---

## AUTH03 Tickets — Admin: API Token Management UI

**Epic:** Admin: API token management UI — ⏸ Deferred (Post-MVP)

> Pushed post-MVP per user direction — not required for initial deployment. The AUTH01 backend (generate/list/revoke `ApiToken`s) already shipped and works fine via direct API calls; this ticket only adds the admin-panel UI on top of it. Absorbs the token-management half of A03 (Webhook & Integration Manager) — see [TASKS.md](TASKS.md#a03).
>
> **Overlap flagged (2026-09-08):** Sprint 9's [MCP06](#mcp06-tickets--frontend-mcp-tokens-admin-screen) builds a `McpTokensScreen` covering this same generate/revoke/copy-once UI, extended for MCP02's `IsReadOnly`/`ExpiresAt`/`RateLimitPerMinute` fields this ticket predates. Not resolved here — flagged for the user to decide whether this ticket should be retired in favor of MCP06 once it ships, or the two kept distinct.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| AUTH03.1 | API token management screen | 🔲 Pending | AUTH01.3, A01 |

---

### AUTH03.1 — API token management screen

**File:** `frontend/packages/web/src/screens/admin/ApiTokensScreen/` (new, lives under the A01 admin shell)

**Acceptance criteria:**
- Table of existing tokens: name, created date, revoked status (from AUTH01.3's list endpoint) — raw token values are never shown here
- "Generate token" flow: prompts for a name, calls AUTH01.3's create endpoint, and displays the raw token **exactly once** in a copy-to-clipboard dialog with an explicit "you won't see this again" warning
- Revoke action per row, with a confirmation step before calling the revoke endpoint
- Screen only reachable through the A01 admin shell (desktop-only guardrail applies)
- Verified in-browser: generate → token shown once, copies correctly, disappears from view on dialog close; revoke → row updates to revoked state and the token no longer authenticates (cross-check against AUTH01.4)

---

## U02 Tickets

**Epic:** Implement FAB quick-capture modal — 🔲 Pending (Sprint 8)

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| U02.1 | FAB button component | 🔲 Pending | — |
| U02.2 | Bottom sheet modal shell | 🔲 Pending | U02.1 |
| U02.3 | Task form fields (title, project, priority, due date) | 🔲 Pending | U02.2 |
| U02.4 | Assignee quick-select row | 🔲 Pending | U02.2 |
| U02.5 | Wire form submit — direct `createTask()` API call + refetch | 🔲 Pending | U02.3, U02.4 |

> All U02 files live in `packages/web/src/`.

---

### U02.1 — FAB Button Component

**Goal:** A floating `+` button anchored bottom-right above the bottom nav, visible on the board screen only.

**Acceptance criteria:**
- `packages/web/src/components/FAB.tsx` — single `onClick` prop
- Positioned `fixed bottom-20 right-4` (above the 64px nav bar)
- 56×56px circle, blue background, white `+` icon
- Subtle drop shadow; scales on press (`active:scale-95`)
- Hidden when `selectedProjectId` is null (no project selected)

**Files:** `packages/web/src/components/FAB.tsx`

---

### U02.2 — Bottom Sheet Modal Shell

**Goal:** Reusable animated slide-up overlay that hosts the capture form.

**Acceptance criteria:**
- `packages/web/src/components/BottomSheet.tsx` — accepts `isOpen`, `onClose`, `children`
- Backdrop dims the screen (`bg-black/50`); tapping backdrop calls `onClose`
- Sheet slides up from bottom on open, slides down on close (CSS transition, no extra animation library)
- Drag handle pill at top of sheet
- Locks body scroll while open
- `max-h-[90vh]` with internal scroll if content overflows

**Files:** `packages/web/src/components/BottomSheet.tsx`

---

### U02.3 — Task Form Fields

**Goal:** The core input fields inside the capture modal.

**Acceptance criteria:**
- `packages/web/src/features/capture/CaptureForm.tsx`
- **Title** — autofocused text input, required, max 300 chars, enter key submits
- **Project selector** — if only one project exists, pre-selected and hidden; otherwise a compact pill-select row showing project name + colour dot
- **Priority picker** — four tappable pills: Low / Medium / High / Critical; Medium pre-selected
- **Due date** — optional; tapping opens the native `<input type="date">`; shows "No due date" when empty with a clear button when set
- All state is local to the form (lifted to U02.5 for submission)

**Files:** `packages/web/src/features/capture/CaptureForm.tsx`

---

### U02.4 — Assignee Quick-Select Row

**Goal:** A horizontal scrollable row of user avatars for one-tap assignment inside the capture modal.

**Acceptance criteria:**
- `packages/web/src/features/capture/AssigneeRow.tsx`
- Loads users from `useUsers()` hook
- Renders a horizontal scroll row of `AssigneeAvatar` (size `md`) components from `@taskmanager/ui`
- First item is "Unassigned" (grey circle with `—`)
- Selected avatar gets a blue ring; tapping a selected avatar deselects (back to Unassigned)
- Row is hidden if there are no active users

**Files:** `packages/web/src/features/capture/AssigneeRow.tsx`

---

### U02.5 — Wire Form Submit

**Goal:** Connect the form to the API and update the board.

**Acceptance criteria:**
- `packages/web/src/features/capture/CaptureModal.tsx` composes `BottomSheet` + `CaptureForm` + `AssigneeRow`
- On submit: call `createTask()` from `@taskmanager/shared`, then call `refetch()` on the tasks hook; close the sheet immediately on success
- Board task list reflects the new task after the refetch
- Submit button disabled while title is empty; shows "Adding…" during the async call
- FAB in `BoardScreen` opens/closes the modal via a single `isOpen` state

**Files:** `packages/web/src/features/capture/CaptureModal.tsx`, updates to `packages/web/src/screens/BoardScreen.tsx`

---

## MCP02 Tickets — ApiToken Scoping

**Epic:** ApiToken Scoping — 🔲 Pending (Sprint 9)

> Adds read-only/expiry/rate-limit scoping to `ApiToken`, ahead of `TaskManager.Mcp` (MCP03) ever calling the API. See [TASKS.md](TASKS.md#mcp02) for the epic summary.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| MCP02.1 | `ApiToken` entity fields + migration | 🔲 Pending | — |
| MCP02.2 | `ApiTokenAuthenticationHandler`: expiry + claims | 🔲 Pending | MCP02.1 |
| MCP02.3 | Write-guard middleware | 🔲 Pending | MCP02.2 |
| MCP02.4 | Rate limiting middleware | 🔲 Pending | MCP02.2 |
| MCP02.5 | `CreateApiTokenCommand` + `ApiTokensController` field support | 🔲 Pending | MCP02.1 |

---

### MCP02.1 — `ApiToken` Entity Fields + Migration

**File:** `backend/TaskManager.Domain/Entities/ApiToken.cs`, `backend/TaskManager.Infrastructure/Data/AppDbContext.cs`

**Goal:** Give `ApiToken` the schema to support read-only scoping, expiry, and rate limiting.

**Acceptance criteria:**
- `ApiToken` gains `bool IsReadOnly` (default `false`), `DateTime? ExpiresAt` (default `null`), `int? RateLimitPerMinute` (default `null`)
- `ApiToken.IsExpired => ExpiresAt is { } exp && exp <= DateTime.UtcNow;` computed property added
- `AppDbContext.ConfigureApiToken` maps the three new columns (nullable/defaulted, no required backfill)
- New migration `ApiTokenScoping` generated via `dotnet ef migrations add ApiTokenScoping` and applies cleanly against a live/throwaway Postgres container with no data loss on existing `ApiTokens` rows
- Existing tokens (created before this migration) default to `IsReadOnly = false`, `ExpiresAt = null`, `RateLimitPerMinute = null` after migration — unchanged behavior for pre-existing tokens

---

### MCP02.2 — `ApiTokenAuthenticationHandler`: Expiry + Claims

**File:** `backend/TaskManager.API/Authentication/ApiTokenAuthenticationHandler.cs`

**Acceptance criteria:**
- A request bearing a token where `matched.IsExpired` is true is rejected (auth fails), same as an unknown/revoked token
- A request bearing a valid, non-expired token gets an `isReadOnly` claim reflecting `ApiToken.IsReadOnly`
- If `RateLimitPerMinute` is set, a `rateLimitPerMinute` claim is added with that value; if null, no claim is added (system default applies)
- Verified live: create a token with `isReadOnly: true` and a 1-minute `expiresAt` via `POST /api/api-tokens` → confirm it authenticates successfully immediately → confirm the `isReadOnly` claim is present → wait for expiry → confirm the same token now fails authentication

---

### MCP02.3 — Write-Guard Middleware

**File:** `backend/TaskManager.API/Middleware/` (new), `backend/TaskManager.API/Program.cs`

**Goal:** Block mutating requests from read-only tokens, uniformly across REST and gRPC.

**Acceptance criteria:**
- New middleware (registered in `Program.cs`'s pipeline, **not** an MVC action filter — action filters don't run for gRPC endpoints) inspects the `isReadOnly` claim on the authenticated principal
- A non-safe REST request (POST/PUT/PATCH/DELETE) from a principal with `isReadOnly = true` is rejected before reaching the controller
- A non-safe gRPC call (e.g. `CreateTask`) from the same principal is rejected the same way — confirmed via a direct gRPC test call, not just REST
- Safe requests (GET, and read-only gRPC RPCs like `ListProjects`/`GetTask`) are unaffected regardless of `isReadOnly`
- Requests from JWT-authenticated (human) users are unaffected — the guard only applies to `ApiToken`-authenticated requests carrying the `isReadOnly` claim
- Verified live: a read-only token can `GET`/list successfully but a `POST`/`CreateTask` (REST) and a gRPC `CreateTask` call both return a clean rejection (403 / `PERMISSION_DENIED`), not a raw 500

---

### MCP02.4 — Rate Limiting Middleware

**File:** `backend/TaskManager.API/Program.cs` (or new `RateLimiting/` folder)

**Goal:** In-process rate limiting per token, covering REST and gRPC identically.

**Acceptance criteria:**
- `Microsoft.AspNetCore.RateLimiting` is wired via `AddRateLimiter`, partitioned by the `apiTokenId` claim
- A token's effective limit is its `rateLimitPerMinute` claim if present, otherwise a documented system default
- Applied via endpoint metadata so it covers REST controllers and gRPC services identically — confirmed by testing both a REST endpoint and a gRPC RPC with the same token
- No Redis or external limiter is introduced — limiting state is in-process only, consistent with MISSION.md's no-cloud-dependencies pillar
- Verified live: create a token with `rateLimitPerMinute: 2`; fire 3 REST calls in a row with that token — the first 2 succeed, the 3rd is rejected with a clean rate-limit response (not a raw 500); repeat against a gRPC RPC with a fresh token and confirm the same behavior

---

### MCP02.5 — `CreateApiTokenCommand` + `ApiTokensController` Field Support

**File:** `backend/TaskManager.Application/ApiTokens/Commands/CreateApiTokenCommand.cs`, `backend/TaskManager.API/Controllers/ApiTokensController.cs`

**Acceptance criteria:**
- `CreateApiTokenCommand` accepts optional `IsReadOnly`, `ExpiresAt`, `RateLimitPerMinute` and persists them on the created `ApiToken`
- `POST /api/api-tokens` request/response DTOs include the three new fields; the raw token value is still only ever returned on the initial create call, unchanged from AUTH01's existing behavior
- Endpoint remains `[Authorize(Roles = "Admin")]` — a non-admin JWT or an `ApiToken`-authenticated caller gets 403
- Verified live: `POST /api/api-tokens` with `isReadOnly: true, expiresAt: "<1 min out>", rateLimitPerMinute: 2` returns a token whose subsequent behavior matches MCP02.2/MCP02.3/MCP02.4 exactly

---

## MCP03 Tickets — Scaffold TaskManager.Mcp

**Epic:** Scaffold TaskManager.Mcp — 🔲 Pending (Sprint 9)

> New standalone `backend/TaskManager.Mcp/` project — one project, not layered like the monolith. See [TASKS.md](TASKS.md#mcp03) for the epic summary.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| MCP03.1 | `TaskManager.Mcp` project scaffold | 🔲 Pending | — |
| MCP03.2 | `AgentPlan`/`AgentStep` entities + `McpDbContext` + initial migration | 🔲 Pending | MCP03.1 |
| MCP03.3 | `GrpcClients/` wrapper | 🔲 Pending | MCP01.1, MCP03.1 |

---

### MCP03.1 — `TaskManager.Mcp` Project Scaffold

**File:** `backend/TaskManager.Mcp/` (new project), `Program.cs`

**Acceptance criteria:**
- New `backend/TaskManager.Mcp/` project added to the solution, referencing `ModelContextProtocol.AspNetCore` (v2.0)
- `Program.cs` wires `builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly()` and `app.MapMcp()`
- Configured via `TASKMANAGER_GRPC_URL` and `MCP_DB_CONNECTION_STRING` environment variables, read via standard ASP.NET Core configuration — no hardcoded values
- `dotnet build backend/TaskManager.Mcp` succeeds
- Running the project locally and connecting with a minimal MCP client (e.g. the SDK's own test client) confirms the server responds to an MCP `initialize` handshake — no tools need to exist yet

---

### MCP03.2 — `AgentPlan`/`AgentStep` Entities + `McpDbContext` + Initial Migration

**File:** `backend/TaskManager.Mcp/Entities/AgentPlan.cs`, `backend/TaskManager.Mcp/Entities/AgentStep.cs`, `backend/TaskManager.Mcp/Data/McpDbContext.cs`

**Acceptance criteria:**
- `AgentPlan` entity: required `ProjectId`, `AgentPlanStatus` enum status, agent ID/session ID metadata fields, timestamps
- `AgentStep` entity: FK to `AgentPlan`, `AgentStepStatus` enum status, ordered within its plan, agent metadata fields, timestamps
- `McpDbContext` (EF Core) is a standalone context with no reference to `TaskManager.Infrastructure` or `TaskManager.Domain` — no FK coupling to the monolith's schema
- Initial migration generated against a new `McpTracking` database on the same Postgres server the monolith uses, confirmed as a distinct database (not a new schema inside the existing `taskmanager` database)
- Migration applies cleanly against a live/throwaway Postgres server: `AgentPlans` and `AgentSteps` tables are created with the expected columns and the `AgentStep → AgentPlan` FK

---

### MCP03.3 — `GrpcClients/` Wrapper

**File:** `backend/TaskManager.Mcp/GrpcClients/` (new)

**Goal:** Prove `TaskManager.Mcp` can reach `TaskManager.API`'s gRPC surface and forward a caller's token correctly.

**Acceptance criteria:**
- `TaskManager.Mcp.csproj` references `TaskManager.Grpc.Contracts` and `Grpc.Net.Client`
- One typed wrapper method per hierarchy RPC (`ListProjects`, `GetProject`, `ListEpicsByProject`, `CreateEpic`, `UpdateEpic`, `ListPhasesByProject`, `ListTasksByProject`, `GetTask`, `CreateTask`, `UpdateTask`, `TransitionTask`), each accepting a bearer token string and forwarding it as gRPC call credentials on the outbound call to `TASKMANAGER_GRPC_URL`
- No MCP tool logic lives here yet (that's MCP04) — this ticket only proves the client can reach `TaskManager.API` and get a typed response back
- Verified live: a throwaway console/test call through the wrapper against a running `TaskManager.API` (with a valid token) returns real project/task data; the same call with no token returns the mapped `UNAUTHENTICATED` failure

---

## MCP04 Tickets — Hierarchy MCP Tools

**Epic:** Hierarchy MCP Tools — 🔲 Pending (Sprint 9)

> Projects/Epics/Tasks/Phases tools exposed through `TaskManager.Mcp`, each forwarding the caller's bearer token and doing no auth logic of its own. See [TASKS.md](TASKS.md#mcp04) for the epic summary.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| MCP04.1 | Project + Epic MCP tools | 🔲 Pending | MCP03.3, MCP02.3 |
| MCP04.2 | Task + Phase MCP tools | 🔲 Pending | MCP03.3, MCP02.3 |

---

### MCP04.1 — Project + Epic MCP Tools

**File:** `backend/TaskManager.Mcp/Tools/ProjectTools.cs`, `backend/TaskManager.Mcp/Tools/EpicTools.cs`

**Acceptance criteria:**
- `list_projects`, `get_project`, `list_epics`, `create_epic`, `update_epic` are registered MCP tools, each extracting the caller's bearer token from the MCP request context and forwarding it via MCP03.3's `GrpcClients/` wrapper
- No token validation logic exists in these tools — an invalid/expired token surfaces as an MCP-level auth error, mapped from the wrapper's `UNAUTHENTICATED` gRPC status
- Calling `create_epic`/`update_epic` with a read-only token (per MCP02) surfaces a clean permission-denied error at the MCP layer, not a raw exception
- Verified live via an MCP test client: `list_projects` and `get_project` return real data for a valid token; `create_epic` creates a real `Epic` row visible via the existing REST API afterward

---

### MCP04.2 — Task + Phase MCP Tools

**File:** `backend/TaskManager.Mcp/Tools/TaskTools.cs`, `backend/TaskManager.Mcp/Tools/PhaseTools.cs`

**Acceptance criteria:**
- `list_tasks`, `get_task`, `create_task`, `update_task`, `transition_task`, `list_phases` are registered MCP tools, following the same token-forwarding pattern as MCP04.1
- `transition_task` called with an invalid status transition returns the same validation error the REST API would, not a generic failure
- Calling `create_task`/`update_task`/`transition_task` with a read-only token surfaces a clean permission-denied error at the MCP layer
- Token lifecycle smoke test verified live: a token with a 1-minute expiry successfully calls `list_tasks` immediately, then fails with a clean auth error once expired — confirmed through an actual MCP tool call, not a direct API call
- Verified live via an MCP test client: `create_task` creates a real `ProjectTask` row visible via the existing REST API afterward; `transition_task` moves it through a valid status chain

---

## MCP05 Tickets — Agent Plan Tracking MCP Tools

**Epic:** Agent Plan Tracking MCP Tools — 🔲 Pending (Sprint 9)

> `AgentPlan`/`AgentStep` tools read/write `McpDbContext` directly — no gRPC hop, no dependency on `TaskManager.API`. See [TASKS.md](TASKS.md#mcp05) for the epic summary.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| MCP05.1 | Plan-level tools (`create_agent_plan`, `get_agent_plan`, `complete_agent_plan`) | 🔲 Pending | MCP03.2 |
| MCP05.2 | Step-level tools + full lifecycle smoke test | 🔲 Pending | MCP05.1 |

---

### MCP05.1 — Plan-Level Tools

**File:** `backend/TaskManager.Mcp/Tools/AgentPlanTools.cs`

**Acceptance criteria:**
- `create_agent_plan` (requires `ProjectId`, agent/session metadata) creates a new `AgentPlan` row directly via `McpDbContext` — no gRPC/API call involved
- `get_agent_plan` returns a plan's current status and its steps (if any) directly from `McpDbContext`
- `complete_agent_plan` transitions a plan to a completed `AgentPlanStatus`
- All three tools reject calls for a `ProjectId`/plan ID that doesn't exist with a clean MCP-level error, not a raw exception
- Verified live: `create_agent_plan` → `get_agent_plan` shows the new plan in its initial status with zero steps

---

### MCP05.2 — Step-Level Tools + Full Lifecycle Smoke Test

**File:** `backend/TaskManager.Mcp/Tools/AgentStepTools.cs`

**Acceptance criteria:**
- `add_agent_step`, `complete_agent_step`, `fail_agent_step` are registered MCP tools, each operating directly on `McpDbContext`
- `add_agent_step` appends a new `AgentStep` to an existing plan; `complete_agent_step`/`fail_agent_step` transition an existing step's `AgentStepStatus`
- Calling a step tool against a step/plan ID that doesn't exist returns a clean MCP-level error
- Full lifecycle smoke test verified live, entirely through `TaskManager.Mcp` (no calls to the main API): `create_agent_plan` → `add_agent_step` (×2) → `complete_agent_step` on one → `fail_agent_step` on the other → `get_agent_plan` shows correct per-step statuses → `complete_agent_plan` succeeds

---

## MCP06 Tickets — Frontend: MCP Tokens Admin Screen

**Epic:** Frontend: MCP Tokens Admin Screen — 🔲 Pending (Sprint 9)

> Mirrors `AllowedOriginsScreen`'s shape, extended for MCP02's new `ApiToken` fields. Overlaps with the still-deferred [AUTH03](#auth03-tickets--admin-api-token-management-ui) — not resolved automatically, see the note on that epic. See [TASKS.md](TASKS.md#mcp06) for the epic summary.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| MCP06.1 | `ApiClient.ts` DTO + CRUD updates | 🔲 Pending | MCP02.5 |
| MCP06.2 | `McpTokensScreen` + `AdminShell` wiring | 🔲 Pending | MCP06.1 |

---

### MCP06.1 — `ApiClient.ts` DTO + CRUD Updates

**File:** `frontend/packages/shared/src/ApiClient.ts`

**Acceptance criteria:**
- `ApiTokenDto` gains `isReadOnly: boolean`, `expiresAt: string | null`, `rateLimitPerMinute: number | null`
- `CreateApiTokenRequest` gains the same three fields as optional inputs
- Existing token-listing/create/revoke functions pass the new fields through; no breaking changes to their call signatures beyond the added optional fields
- `tsc --noEmit` passes clean in `packages/shared`

---

### MCP06.2 — `McpTokensScreen` + `AdminShell` Wiring

**File:** `frontend/packages/web/src/screens/admin/McpTokensScreen/` (new), `frontend/packages/web/src/screens/admin/AdminShell.tsx`

**Acceptance criteria:**
- `McpTokensScreen` mirrors `AllowedOriginsScreen`'s shape: a table of existing tokens (name, read-only badge, expiry, rate limit, created date, revoked status) plus an add form and a per-row revoke action
- Add form supports setting a name, a read-only toggle, an optional expiry date, and an optional rate-limit-per-minute number
- Raw token value is shown exactly once immediately after creation, in a copy-to-clipboard dialog with an explicit "you won't see this again" warning, and is never retrievable again afterward
- `AdminShell.tsx` gains an `'mcpTokens'` screen entry and nav item, following the same pattern as the other admin screens; reachable only through the existing desktop-only admin guardrail (A01)
- `npm run build --workspace=packages/shared` then `--workspace=packages/web` both succeed
- Verified in-browser: Admin → MCP Tokens → create a token with read-only + an expiry set, see the raw value once, confirm the values persist correctly in the table afterward, revoke it, confirm it disappears from active/authenticating use (cross-check against MCP02.2's expiry/revocation behavior)

---

## MCP07 Tickets — Containerize & Wire into docker-compose

**Epic:** Containerize & Wire into docker-compose — 🔲 Pending (Sprint 9)

> `TaskManager.Mcp` Dockerfile + `docker-compose.yml` service entry — MCP HTTP endpoint exposed over Tailscale/LAN, gRPC leg and `McpTracking` Postgres stay internal-only. See [TASKS.md](TASKS.md#mcp07) for the epic summary.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| MCP07.1 | `TaskManager.Mcp` Dockerfile | 🔲 Pending | MCP03.1 |
| MCP07.2 | `docker-compose.yml` service entry | 🔲 Pending | MCP07.1 |
| MCP07.3 | Internal-only surface check + end-to-end integration verification | 🔲 Pending | MCP07.2 |

---

### MCP07.1 — `TaskManager.Mcp` Dockerfile

**File:** `backend/TaskManager.Mcp/Dockerfile` (new)

**Acceptance criteria:**
- Multi-stage Dockerfile: SDK stage publishes `TaskManager.Mcp` in Release, runtime stage runs it on the ASP.NET Core runtime image — consistent with how `TaskManager.API` is built in the D01–D03 monolithic image's `build-api` stage
- Runs as a non-root user, matching the D02 monolithic image's `appuser` convention
- `docker build` against the new Dockerfile succeeds and produces a runnable image
- Running the image standalone (with `TASKMANAGER_GRPC_URL`/`MCP_DB_CONNECTION_STRING` env vars pointed at a reachable API/Postgres) starts cleanly and responds to an MCP `initialize` handshake

---

### MCP07.2 — `docker-compose.yml` Service Entry

**File:** `docker-compose.yml`; root `README.md` gains a short mention

**Acceptance criteria:**
- A new `mcp` service entry builds from `backend/TaskManager.Mcp/Dockerfile`, sets `TASKMANAGER_GRPC_URL` to the monolith's internal gRPC port and `MCP_DB_CONNECTION_STRING` to the `McpTracking` database on the same Postgres instance
- Only the MCP service's HTTP (MCP protocol) port is published/exposed on the host — no port mapping is added for the gRPC leg or the `McpTracking` Postgres connection; both stay reachable only over the internal Docker network
- `docker compose config` succeeds with the new service present (valid YAML/env-var interpolation, not just a comment that looks right)
- Root `README.md`'s "Running it" section gains a short mention of the MCP service and its Tailscale/LAN-reachable endpoint

---

### MCP07.3 — Internal-Only Surface Check + End-to-End Integration Verification

**Goal:** Prove the whole MCP integration works end-to-end against real running containers, not simulated.

**Acceptance criteria:**
- Full solution build: `dotnet build` succeeds across `TaskManager.Domain`, `TaskManager.Application`, `TaskManager.Infrastructure`, `TaskManager.API`, `TaskManager.Grpc.Contracts`, and `TaskManager.Mcp` together
- Internal-only surface confirmed live: with the full `docker-compose.yml` stack running, the API's gRPC port and the `McpTracking` Postgres connection are unreachable from outside the Docker network (e.g. attempting to connect from the host to the gRPC port fails/times out) — only the MCP HTTP endpoint and the existing monolith ports (per D01–D03/D04) are reachable
- MCP end-to-end test: point a real MCP client (Claude Code's own `mcp` config, or the SDK's test client) at the running `mcp` service with a valid token; `list_projects`, `create_task`, and `create_agent_plan` all succeed end-to-end through the full container stack
- The same test repeated with a revoked token fails cleanly at the MCP layer, not a hang or a raw 500
- Findings (pass/fail per item above) reported back to the user before this ticket is marked Done

---

## D04 Tickets — Disable Bundled Postgres Toggle

**Epic:** Disable Bundled Postgres Toggle — 🔲 Pending (Sprint 10)

> Extends D01–D03's monolithic production image — see [CompletedTickets.md](CompletedTickets.md#d01d03-tickets--monolithic-production-docker-image) for the image this builds on. Graduated out of the Post-MVP Backlog per user direction (2026-09-01) — was previously logged under [ROADMAP.md](ROADMAP.md#post-mvp-backlog) / [TASKS.md](TASKS.md#-post-mvp-backlog)'s "Docker: Disable Bundled Postgres" item.
>
> **Background (from the person who built D01–D03):** the s6-overlay service tree lives at `docker/s6-overlay/s6-rc.d/`, with the actual logic in `docker/s6-overlay/scripts/`. `postgres-init` (oneshot) runs `postgres-init.sh`, which does `initdb`/`createdb` on first boot only (already idempotent — skips if the data directory has content). `postgres` (longrun) runs `postgres-run.sh`, which execs `postgres -D $PGDATA ...` under `s6-setuidgid appuser`. `api` (longrun) runs `api-run.sh`, which currently *always* polls `pg_isready -h localhost -p 5432` in a loop before starting the API — hardcoded to the local bundled Postgres regardless of what `DatabaseSettings__ConnectionString` actually points at.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| D04.1 | s6-overlay: `DISABLE_BUNDLED_POSTGRES` toggle | 🔲 Pending | — |
| D04.2 | `docker-compose.yml` example + docs | 🔲 Pending | D04.1 |

---

### D04.1 — s6-overlay: `DISABLE_BUNDLED_POSTGRES` Toggle

**File:** `docker/s6-overlay/scripts/postgres-init.sh`, `docker/s6-overlay/scripts/postgres-run.sh`, `docker/s6-overlay/scripts/api-run.sh`

**Goal:** When `DISABLE_BUNDLED_POSTGRES` is set, the monolithic image skips initializing and starting the bundled Postgres entirely, and the API starts without waiting on a local Postgres readiness check — for deployments that always point at an external Postgres via `DatabaseSettings__ConnectionString`.

**Acceptance criteria:**
- `postgres-init.sh` (oneshot): when `DISABLE_BUNDLED_POSTGRES` is set, the script exits `0` immediately, before any `initdb`/`createdb` call — s6-rc reports `postgres-init` as complete and no changes are made to the data directory
- `postgres-run.sh` (longrun): when `DISABLE_BUNDLED_POSTGRES` is set, the script execs `pause` (the execline no-op bundled with s6-overlay, same distribution as `s6-setuidgid`) instead of execing `postgres` — s6-rc shows the `postgres` service as up/stable, **not** crash-looping, since a longrun that just exits is treated as a crash by s6-rc; `ps` inside the running container shows no `postgres` process
- `postgres-run.sh`: when `DISABLE_BUNDLED_POSTGRES` is unset (the default), behavior is unchanged — execs `postgres -D $PGDATA ...` under `s6-setuidgid appuser` exactly as today
- `api-run.sh`: when `DISABLE_BUNDLED_POSTGRES` is set, the `pg_isready -h localhost -p 5432` polling loop is skipped entirely and the script proceeds straight to starting the API
- `api-run.sh`: when `DISABLE_BUNDLED_POSTGRES` is unset (the default), the existing `pg_isready` wait loop is unchanged
- Verified live: build and run the image with `DISABLE_BUNDLED_POSTGRES=true` and `DatabaseSettings__ConnectionString` pointing at a real external Postgres — all four s6 services (`postgres-init`, `postgres`, `api`, `nginx`) report up, no local `postgres` process is running in the container, and the API successfully connects to and migrates the external database
- Verified live: build and run the image with `DISABLE_BUNDLED_POSTGRES` unset — behavior matches pre-D04 exactly (bundled Postgres initializes and starts, the API waits on it before starting) — no regression

**Implementation status (2026-09-01): implementation complete, awaiting user sign-off.** Status stays 🔲 Pending until confirmed.

This turned out to be a real bug fix hit in practice, not just the backlog item's original "resource waste" framing — pointing the container at an external Postgres left the API endlessly logging "waiting for postgres" while the external Postgres showed zero incoming connections, only starting "after a few restarts." Root cause: `api-run.sh` polled `pg_isready` against the *bundled local* Postgres unconditionally, regardless of what `DatabaseSettings__ConnectionString` actually pointed at — the API was blocked on an irrelevant local readiness check, and only appeared to "work" once the unrelated bundled instance coincidentally finished its own init.

Implemented exactly as ticketed, with one correction found during verification: the ticket's background text said to exec `pause`, but the actual s6-overlay utility is `s6-pause` (confirmed present at `/command/s6-pause` in the built image) — used the correct name.
- `postgres-init.sh` — exits `0` immediately when `DISABLE_BUNDLED_POSTGRES` is set, before the appuser re-exec or any `initdb`/`createdb` call.
- `postgres-run.sh` — execs `s6-pause` instead of `postgres` when the var is set; unchanged when unset.
- `api-run.sh` — the `pg_isready` wait loop is now wrapped in `if [ -z "$DISABLE_BUNDLED_POSTGRES" ]; then ... fi`, so it's skipped entirely when disabled; unchanged when unset.

Verified live with real containers, not simulated: rebuilt the image; spun up a real throwaway external Postgres container on a shared Docker network; ran the TaskManager image with `DISABLE_BUNDLED_POSTGRES=true` pointed at it — `postgres-init` logged skipping and exited immediately, the `postgres` service supervised `s6-pause` (confirmed via `ps aux` inside the container — no real `postgres` process running at all), the API started immediately with no "waiting for postgres" delay, `/health` went healthy within seconds, and — the key proof — the external Postgres's own logs showed the real EF Core migration bootstrap query hitting it directly, with `\dt` confirming all 10 real application tables plus `__EFMigrationsHistory` were created and migrated there. Regression check with `DISABLE_BUNDLED_POSTGRES` unset confirmed byte-for-byte the same behavior as the original D01-D03 verification — bundled Postgres initializes normally, the `pg_isready` wait loop runs as before, real `postgres` processes run under `appuser`, health goes healthy. No regression. All test containers, volumes, and the test network were cleaned up afterward.

---

### D04.2 — `docker-compose.yml` Example + Docs

**File:** `docker-compose.yml`; `.env.example` reviewed but left untouched (see criteria below); root `README.md` gains a mention

**Goal:** A deployer pointing at an external Postgres can discover and enable `DISABLE_BUNDLED_POSTGRES` without reading source or scripts.

**Acceptance criteria:**
- `docker-compose.yml`'s existing commented external-Postgres example block (currently just `DatabaseSettings__ConnectionString`) gains a commented `# - DISABLE_BUNDLED_POSTGRES=true` line alongside it, with a one-line comment noting it skips starting the bundled Postgres service
- `.env.example` is confirmed to stay untouched by this ticket — it documents only `JWT_SECRET` (a required secret); `DISABLE_BUNDLED_POSTGRES` is an optional, non-secret compose var and belongs in `docker-compose.yml` next to its sibling `DatabaseSettings__ConnectionString` example, not in `.env.example`
- A root `README.md` already exists (added alongside the GitHub Actions release pipeline, 2026-08-31) — it gains a short mention of `DISABLE_BUNDLED_POSTGRES` in its "Running it" section, next to or near the external-Postgres guidance
- Verified: `docker compose config` succeeds with the new line uncommented alongside `DatabaseSettings__ConnectionString` (confirms valid YAML/env-var interpolation, not just a comment that looks right)

---

## U03 Tickets

**Epic:** Add gesture-driven task interactions — ⏸ Deferred (Mobile App Phase)

Gesture interactions (swipe-to-complete, swipe-to-assign) are touch-native features that belong in `packages/mobile`. Deferred until the mobile Capacitor build is activated post-MVP.

---
