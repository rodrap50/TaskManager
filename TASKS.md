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

| # | Epic | Status | Blocked By |
|---|---|---|---|
| 13 | [M01 — Scaffold MCP server](#m01) | 🔲 Pending | #11 |
| 14 | [M02 — Implement core MCP tools](#m02) | 🔲 Pending | #13 |
| 15 | [M03 — Implement agent plan tracking MCP tools](#m03) | 🔲 Pending | #14 |
| 16 | [M04 — Containerize MCP server & wire into docker-compose](#m04) | 🔲 Pending | #13 |

---

## 📋 Epic Descriptions

### U01
**Build Kanban board & list dashboard** — ✅ Done
Two view modes (Kanban columns + prioritized list), data hooks, shared task card components, view toggle persisted to localStorage. See [CompletedTickets.md](CompletedTickets.md#u01-tickets) for the full ticket history.

---

### BUG-04
**Fix API base URL / create .env.development**
`ApiClient.ts` falls back to `http://localhost:5000` but the backend dev profile runs on port `5062`. No `.env.development` exists. Create `packages/web/.env.development` with `VITE_API_URL=http://localhost:5062` and `packages/web/.env.example` documenting it.

---

### BUG-01
**Add JsonStringEnumConverter to backend**
`AddControllers()` has no JSON options. `System.Text.Json` rejects string enum values by default, so every write endpoint (`CreateProject`, `CreateTask`, `UpdateTask`, `TransitionTask`) returns a deserialization error. Chain `.AddJsonOptions()` with `JsonStringEnumConverter` in `Program.cs`.

---

### BUG-05
**Show errors in CreateProjectModal** — ✅ Done
`handleSubmit` had no `catch` block — API errors were silently swallowed via `void handleSubmit()`. Added error state to the modal; renders a red inline error message on failure and clears on retry. Verified happy/failure/retry paths in-browser.

---

### BUG-06
**Show API load errors in screens** — ✅ Done
`useProjects` and `useTasks` returned an `error` field that neither `ProjectSelectorScreen` nor `BoardScreen` rendered. Failed loads looked identical to empty data. Both screens now show "Failed to load — check the API is running" when `error` is non-null. Verified happy path and forced-failure path in-browser for both screens.

---

### ARCH01
**npm workspace monorepo restructure** — ✅ Done
Replaced the single Vite app with an npm workspaces monorepo (`frontend/packages/*`):
- `@taskmanager/shared` — axios API client + all TypeScript DTOs
- `@taskmanager/ui` — pure presentational components (TaskCard, PriorityBadge, StatusIndicator, AssigneeAvatar)
- `@taskmanager/web` — always-online WebUI; direct API calls, no SQLite/Capacitor
- `@taskmanager/mobile` — preserved Capacitor + jeep-sqlite + offline-first sync code; deferred to post-MVP

The split resolves the WASM crash (jeep-sqlite MIME type issue in dev) and cleanly separates web-first from mobile-first concerns. See [CompletedTickets.md](CompletedTickets.md#arch01-tickets) for tickets.

---

### DS01
**Design system: tokens, Tailwind config, CSS vars** — ✅ Done
Established the full visual foundation before new screen work. Since the project runs Tailwind v4 (CSS-first, no `tailwind.config.ts`), the zinc/crimson palette and dark/light mode split were implemented as an `@theme` block + `.light` class override in `index.css` rather than a JS config file — see [CompletedTickets.md](CompletedTickets.md#ds01-tickets--design-system-foundation) for the full rationale. A temporary theme toggle in `App.tsx` (to be replaced by the real Settings screen) proves the token swap end-to-end. All subsequent components should consume these tokens (`bg-surface`, `text-text`, `border-border`, `bg-primary-800`, etc.) — no hardcoded Tailwind color classes.

---

### DS02
**Restyle app shell & ProjectSelectorScreen** — ✅ Done
Applied the new design system to the existing working screen as a proving ground. App shell ended up pivoting from a conventional header bar to a chrome-less floating-pill shell (see [MISSION.md](MISSION.md#-design--style-decisions)); project cards got a centered `max-w-2xl` layout with color accent bars and a static crimson shadow; the Create Project modal got token-based styling plus a curated, muted color picker. Dark and light mode both verified throughout. See [CompletedTickets.md](CompletedTickets.md#ds02-tickets--app-shell--projectselectorscreen-restyle) for the full ticket history including follow-up rounds.

---

### DS03
**Build Board screen (Kanban view)** — ✅ Done
Built the Kanban board screen from scratch in the new design system. Columns map to task statuses. Task cards use the new `TaskCard` component. Routes from project selection. Native HTML5 drag-and-drop wires status transitions directly; a `@dnd-kit/core` migration was considered and deferred (tracked as **MOB04**, post-MVP). Also fixed BUG-07 (Done/Cancelled tasks can only reopen to Backlog — the board now visually locks out invalid drop targets during a drag) and added a toast on failed transitions. See [CompletedTickets.md](CompletedTickets.md#ds03-tickets) for the full ticket history.

---

### DS04
**Task creation modal (web, inline — not FAB)** — ✅ Done
Web-native way to create a task: a modal dialog triggered from the board screen (not a FAB, which is mobile-deferred). Fields: title, priority, optional due date, optional assignee (project is implicit from board context). A reusable `Modal` shell (reused by `CreateProjectModal`-style crimson `panelGlow`) hosts `TaskForm`, with the assignee selector built inline per the ticket's own file path. Wired into `BoardScreen` via a "+ New Task" button that opens `CreateTaskModal`, which calls `createTask()` and refetches so the board updates immediately. See [CompletedTickets.md](CompletedTickets.md#ds04-tickets) for the full ticket history.

---

### DS05
**Task detail panel** — ✅ Done
Slide-in panel (`features/tasks/TaskDetailPanel.tsx`) for viewing and editing a task, reachable from both the Kanban board and List view. Fields: title, status, priority, phase, assignee, due date, description. Defaults to a **read-only view**; an **Edit** button switches into edit mode where changes are staged locally, and **Save** batches everything into a single `transitionTask()` call (if status changed) plus one `updateTask()` call, showing a single inline error on failure — **Cancel** discards staged edits with no API call. (Reworked from the original ticket's "every field mutates immediately, no Save button" design based on direct user feedback after shipping — see the DS05.3/DS05.4 follow-ups in [CompletedTickets.md](CompletedTickets.md#ds05-tickets).) Replaces the old bottom-sheet-style panel that lived under `features/board/TaskDetailPanel/`. Also fixes BUG-02 (phase highlight now tracked via local state instead of the stale `task` prop) and BUG-03 (panel takes a `refetch` prop and calls it on close, so board data never goes stale after an edit). See [CompletedTickets.md](CompletedTickets.md#ds05-tickets) for the full ticket history.

---

### DS06
**Restyle List view in the new design system** — ✅ Done
Applied design tokens to `ListView`'s rows/chrome (DS06.1). DS06.2's original premise — that `ListView` had its own embedded `TaskDetailPanel` needing consolidation onto DS05's standalone one — turned out to be stale by the time this epic was reached: DS05 shipped after DS06's tickets were originally written, and it already gave `BoardScreen` a single shared `TaskDetailPanel` instance used by both Kanban and List view, so there was never a second panel to consolidate away. DS06.2 closed as a no-op-but-verified ticket confirming that architecture live (same component instance opens from both views, edits persist, and BUG-03's refetch-on-close works correctly on the List surface). See [CompletedTickets.md](CompletedTickets.md#ds06-tickets--restyle-list-view) for the full ticket history.

---

### A01
**Build desktop admin panel shell** — ✅ Done
Desktop-only layout wrapper with side navigation. Verified directly against source (2026-08-23) — built but never marked done in these docs until now. `AdminGuard.tsx` locks out sub-1024px viewports with a live-resize `matchMedia` listener (not just initial mount), rendering a plain "Admin requires a desktop screen" message instead of any admin content. `AdminShell.tsx` provides the side-nav + content-area chrome, hosting `UserManagementScreen` (A02). Entry point is an `isAdmin`-gated button in `FloatingPill` (`onOpenAdmin`), fully absent from the DOM for non-admins. See [CompletedTickets.md](CompletedTickets.md#a01-tickets--admin-panel-shell--guardrails) for tickets (A01.1–A01.3, 10 points).

---

### A02
**Build user management portal** — ✅ Done
Admin screen: table of all `AppUser` records via `useUsers()`, wired into `AdminShell` (A01.2) in place of its earlier placeholder. `UsersController` only had `GetUsers`/`GetById`/`CreateUser` — the status/role/avatar endpoints didn't exist yet despite the domain layer already supporting them (`Deactivate`/`Reactivate`/`PromoteToAdmin`/`RevokeAdmin`/`UpdateAvatarUrl`), so this epic included real backend work, not just UI: `PUT /api/users/{id}/active`, `PUT /api/users/{id}/admin`, and `POST /api/users/{id}/avatar` (new `IAvatarStorage`/`LocalAvatarStorage`, mirroring the existing `IPasswordHasher` pattern, storing uploads under `wwwroot/avatars/{id}.{ext}`). Frontend: user list, create-user modal, per-row active/admin toggle and avatar-upload actions, using a new `getErrorMessage()` helper (`ApiClient.ts`) so real backend error text surfaces inline instead of axios's generic message.

Two real bugs surfaced during user testing and were fixed within this epic: (1) promoting a second admin threw a raw `500` because AUTH02.3's single-admin Postgres constraint (`IX_Users_SingleAdmin`) wasn't accounted for by the new promote endpoint — user confirmed intent (exactly one admin, always) and the handler now returns a clean `422` instead of letting the DB error bubble up. This leaves a known, currently-open gap: **there is no way to ever change who the single admin is**, since promote is always blocked once an admin exists and revoke is always blocked when it's the last one — an atomic "transfer admin" operation would be needed and hasn't been built; logged in [Post-MVP Backlog](#-post-mvp-backlog). (2) avatars weren't rendering anywhere in the app — a latent origin-mismatch bug in pre-existing avatar-rendering code (a relative `avatarUrl` resolving against the Vite dev server's origin instead of the API's), surfaced by A02.6 since it was the first feature to actually populate a real `avatarUrl`. Fixed via a new `resolveAssetUrl()` helper applied everywhere `AssigneeAvatar` renders a user's avatar. See [CompletedTickets.md](CompletedTickets.md#a02-tickets--user-management-portal) for the full ticket-by-ticket history (A02.1–A02.6, 17 points).

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
All seven bug tickets are resolved: BUG-01/04/05/06 (Sprint 1, app-breaking — JSON enum converter, API base URL, silent error swallowing, missing load-error display) and BUG-02/03/07 (Sprint 3, UX polish — stale phase highlight and stale list rows, both fixed as part of DS05.2/DS05.4; Done/Cancelled drag-drop restriction, fixed as part of DS03.4). No open bugs remain. Full ticket history archived at [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking) and [CompletedTickets.md](CompletedTickets.md#sprint-3-bug-tickets--ux-polish-pre-deployment).

---

### PREP
**Production readiness & environment config** — ✅ Done (signed off 2026-08-30)
Hardened the app for production before Dockerising. Verified against current source before ticketing (2026-08-28) — all six items confirmed open at that time. Closed out in two rounds: PREP.1/PREP.2 signed off 2026-08-29, PREP.3–PREP.6 signed off 2026-08-30. Closes out Sprint 6 in full (paired with BUGS, already done) — this also completes [ROADMAP.md](ROADMAP.md#phase-8-bug-fixes--deployment-prep) Phase 8.
- **PREP.1** — Locked CORS to a DB-backed, admin-editable allow-list of known origins (removed `AllowAnyOrigin`) — scope grew mid-implementation past the original static-config plan into a full `AllowedOrigin` entity + admin screen.
- **PREP.2** — Added a `/health` endpoint to the backend for Docker health checks.
- **PREP.3** — Verified `VITE_API_URL` env var wiring for the web container — no `.env.production` created; documented in `.env.example` instead since the real production URL is deployment-specific and still unknown pending Phase 9's D03.
- **PREP.4** — Reviewed connection-string handling for Docker secrets/env vars — confirmed the env-var override already works via standard ASP.NET Core config precedence, no secrets committed; also added an approved scope-addition `Program.cs` startup guard that fails fast in Production if the DB connection string or JWT secret aren't configured.
- **PREP.5** — Confirmed the Serilog console sink is appropriate for container stdout; also added an approved scope-addition of friendly `SEQ_URL`/`LOG_FILE_PATH` env vars to repoint the File and Seq sinks, both staying always-registered by default.
- **PREP.6** — Deleted the legacy pre-monorepo `frontend/src/` tree (dead since the ARCH01 workspace split) along with its orphaned root-level config (`frontend/vite.config.ts`, `index.html`, `tsconfig*.json`, `tailwind.config.js`, `postcss.config.js`, `capacitor.config.ts`); also deleted three more confirmed-dead root items as an approved scope addition (`frontend/public/`, `frontend/dist/`, `frontend/eslint.config.js`). A pre-existing, unrelated `packages/web` favicon 404 was noted but left unfixed as out of scope.

See [CompletedTickets.md](CompletedTickets.md#prep-tickets--production-readiness--environment-config) for the full ticket-by-ticket history (PREP.1–PREP.6, 12 points).

---

### DM01
**Backend: Epic entity** — ✅ Done (signed off 2026-08-26)
New `Epic` domain entity as a child of `Project` — sibling of the existing `ProjectPhase`, not a replacement, not nested inside it. Full CQRS CRUD mirroring the Phase pattern exactly: Domain entity + `Project.AddEpic`/`RemoveEpic`, single optional `EpicId` FK on `ProjectTask`, EF configuration + migration, `IEpicRepository`/`EpicRepository`, `Epics/` CQRS folder (Create/Update/Delete/GetByProject), `EpicsController`. Also removed `Epic` from the `ProjectScope` enum (an "Epic" is no longer a kind of `Project`) and reset the dev DB for the enum renumbering — the dev-DB assumption check (part of DM01.2's own acceptance criteria) caught that the DB actually held real-looking data at the time, not the empty state the ticket assumed; flagged to the user, who confirmed a full reset was fine anyway. Confirmed free-form like Phase — no fixed/hardcoded set, full per-project CRUD, optional per task. Full researched plan: [EPIC-ENTITY-PLAN.md](EPIC-ENTITY-PLAN.md). See [CompletedTickets.md](CompletedTickets.md#dm01-tickets--backend-epic-entity) for the full ticket-by-ticket history (DM01.1–DM01.4, 16 points).

---

### DM02
**Frontend: Epic entity consumption** — ✅ Done (signed off 2026-08-26)
Shared `EpicDto` + `CreateEpicRequest`/`UpdateEpicRequest` types and CRUD functions in `@taskmanager/shared`, mirroring the existing Phase functions. New `useEpics` hook (mirrors `usePhases`). Epic picker added to `features/tasks/TaskDetailPanel.tsx` (corrected during ticketing — the original plan's `ListView.tsx`-embedded panel no longer exists post-DS05/DS06), directly below the existing Phase picker, **scope-gated to `Project`-scope projects only** — hidden entirely for DailyTask projects, per user direction (see DM03). `CreateProjectModal`'s `SCOPE_OPTIONS` drops `'Epic'`.

Expanded twice after the original 5 tickets shipped, both per direct user request: **DM02.6** added a full Epics management panel (create/rename/delete) after the user pointed out the original ticketed boundary ("no manage-Epics screen, read + assign only") left no way to actually create an epic anywhere in the UI — the same latent gap Phase has always had, never surfaced until Epic did. **DM02.7** fixed a Kanban/List inconsistency the user spotted (the Kanban board's `TaskCard` never rendered a phase or epic badge, unlike `ListView`) and added epic badge coloring — each epic is now assigned a random color from the same palette used for Projects (excluding the owning project's own color) at creation, with both the Kanban and List badges rendered in that color, and the Kanban badge repositioned to the card's top-right corner. See [CompletedTickets.md](CompletedTickets.md#dm02-tickets--frontend-epic-entity-consumption) for the full ticket-by-ticket history (DM02.1–DM02.7, 12+ points).

---

### DM03
**DailyTask projects become a simplified todo list** — ✅ Done (signed off 2026-08-26)
Added mid-planning alongside the Epic entity work since both touch `ProjectScope`/project-type behavior. DailyTask-scope projects show only a list view (no Kanban/`ViewToggle` at all — not even as an option), tasks use a simple complete/incomplete model (reuses the existing `TaskStatus` enum: checking = `Done`, unchecking = `Backlog`, no new field needed — lines up neatly with the existing Done-can-only-reopen-to-Backlog domain rule from BUG-07), and DailyTask projects get no Phases at all, extending naturally to no Epics either via DM02.4's scope-gating. Enforced at the UI layer only, consistent with how other cross-cutting UI rules work in this app (e.g. A01's desktop guardrail) — not enforced server-side. Closes out Sprint 3 in full. See [CompletedTickets.md](CompletedTickets.md#dm03-tickets--dailytask-simplified-todo-list) for the full ticket-by-ticket history (DM03.1–DM03.3, 9 points).

---

### AUTH01
**Backend: Authentication & API tokens** — ✅ Done (Sprint 4, MVP-Required)
JWT login (`POST /api/auth/login`) validated against the existing BCrypt hash, with a global JWT Bearer + `[Authorize]` fallback policy locking down every other endpoint by default. A new `ApiToken` entity gives non-human/service accounts (Home Assistant, automation scripts) admin-managed credentials, authenticated via an `X-Api-Token` header side-by-side with JWTs through a policy-scheme selector. `POST /api/users` is restricted to admins to close a privilege-escalation gap AUTH01.2 introduced; `GET /api/users`/`{id}` were deliberately left open since they back assignee/member pickers used by any logged-in user. See [CompletedTickets.md](CompletedTickets.md#auth01-tickets--backend-authentication--api-tokens) for the full ticket-by-ticket history (AUTH01.1–AUTH01.5).

---

### AUTH02
**Frontend: Login & session** — ✅ Done (Sprint 4, MVP-Required)
`LoginScreen` (username/password, styled like `CreateProjectModal`'s card) calls AUTH01's login endpoint. A response interceptor on the shared axios instance attaches the bearer token to every request and dispatches a `tm:unauthorized` event on any non-login 401. `AuthContext` (`frontend/packages/web/src/App/AuthContext.tsx`) decodes the stored JWT client-side into `{ id, username, isAdmin }`, exposes `signIn(token)`/`logout()`, and listens for that event to clear the session automatically. `App.tsx`'s `AppContent` is the single choke point — no protected screen mounts unless `user` is non-null. A Log out button lives in `FloatingPill` next to the theme toggle.

Expanded mid-sprint to also close a gap AUTH01.5 surfaced: once `POST /api/users` requires an existing admin, a fresh deployment has no way to create the very first one. **AUTH02.3** adds a one-time `POST /api/auth/setup` (`[AllowAnonymous]`, DB-level-guarded via a Postgres partial unique index against ever creating a second admin, even under concurrent double-submission — verified with 10 truly simultaneous requests) plus a `GET /api/auth/setup/status` check. **AUTH02.4** adds a `SetupScreen` the frontend shows instead of `LoginScreen` only while setup is still required, re-checking status on every load so it can't be reopened once an admin exists. See [CompletedTickets.md](CompletedTickets.md#auth02-tickets--frontend-login--session) for the full ticket-by-ticket history (AUTH02.1–AUTH02.4).

---

### AUTH03
**Admin: API token management UI** — ⏸ Deferred (Post-MVP)
Admin screen to generate/revoke `ApiToken`s (from AUTH01). Token value is shown once at creation with a copy-to-clipboard action and never displayed again. Absorbs the token-management half of A03 — A03 becomes this backend's UI consumer rather than duplicating it. Pushed post-MVP per user direction — AUTH01's backend already works standalone via direct API calls, so this is UI-only remaining scope. See [TICKETS.md](TICKETS.md#auth03-tickets--admin-api-token-management-ui) for the still-specced ticket, ready to pick up post-deployment.

---

### PM01
**Backend: Project membership** — MVP-Required (Sprint 4) — ✅ Done (PM01.1–PM01.3 signed off 2026-08-24)
New `ProjectMember` join entity (`Project` ↔ `AppUser`) — flat membership, no per-project roles; admin powers reuse the existing global `AppUser.IsAdmin` (see [ROADMAP.md](ROADMAP.md#post-mvp-backlog) for the deferred RBAC backlog item). `Project.AddMember`/`RemoveMember` domain methods. Whoever creates a project is automatically added as a member — requires the current-user-id from AUTH01/AUTH02, so `CreateProjectCommand` needs updating. CQRS + controller for add/remove/list members. See [TICKETS.md](TICKETS.md#pm01-tickets--backend-project-membership) for tickets.

PM01.1 (`ProjectMember` entity + migration) is implemented: flat entity (no `AuditInfo`), `Project.AddMember(userId)`/`RemoveMember(userId)` (idempotent, constructs the join row internally per the ticket's literal signature), migration `AddProjectMember` (`Cascade` from `Projects`, `Restrict` from `Users`, unique `(ProjectId, UserId)` index). Verified against a live Postgres container: migration applies cleanly to the real dev DB (schema-only); on an isolated throwaway container, duplicate `(ProjectId, UserId)` correctly rejected and deleting a `Project` cascades to its `ProjectMember` rows.

PM01.2 (auto-assign creator as member) is implemented: new `ICurrentUserService` (Application interface, API-layer implementation backed by `IHttpContextAccessor`) reads the current user id from the JWT or `ApiToken` claim. `CreateProjectCommand` no longer accepts a client-supplied `CreatedByUserId` at all — the handler derives it from `ICurrentUserService` and calls `project.AddMember(ownerId)` in the same `SaveChanges` call that creates the project. Deleted the now-dead `WellKnownUsers.cs`. Also refactored `ApiTokensController` onto the same shared service. Frontend: removed `createdByUserId` from `CreateProjectRequest` and from `CreateProjectModal`'s create call. Verified against an isolated throwaway container and through the real browser UI: creating a project always produces exactly one matching `ProjectMembers` row, with no separate call, and no project can end up with zero members.

PM01.3 (CQRS + controller for add/remove/list members) is implemented: extended `ProjectsController` with `POST`/`DELETE`/`GET` on `/api/projects/{projectId}/members`, backed by a new `Projects/Members/` CQRS folder. Found and fixed a real EF Core bug during verification, not just an implementation detail: `Project.AddMember` adds a `ProjectMember` via collection-navigation on an already-tracked project, which EF Core's default `Guid`-key convention misread as an update to an existing row rather than a new insert (client-set key + no explicit `.Add()` call = EF guesses `Modified` instead of `Added`). Fixed with `.Property(m => m.Id).ValueGeneratedNever()` in `AppDbContext.cs` — the standard fix for this exact scenario, no migration needed (metadata-only, confirmed by generating then removing an empty migration). This same landmine would have hit `AddPhase`/`AddTask` too, if either had ever actually been called this way. Verified live against a throwaway container: add, list, idempotent re-add, remove, remove-the-creator-with-no-protection, 404s, and the 401 check all pass.

---

### PM02
**Frontend: Project membership UI** — MVP-Required (Sprint 4) — ✅ Done (PM02.1–PM02.3 signed off 2026-08-25)
"Members" panel reachable from within the project itself (not the admin panel, per user direction) — lists current members with avatars, supports add/remove via a search over existing `AppUser`s (`useUsers()`). See [TICKETS.md](TICKETS.md#pm02-tickets--frontend-project-membership-ui) for tickets.

New right-side slide-in `MembersPanel` (`frontend/packages/web/src/features/project/MembersPanel/`), cloned structurally from `TaskDetailPanel`'s drawer pattern, reachable via a "Members" button on `BoardScreen` next to `ViewToggle`. Backed by a new read-only `useProjectMembers` hook (mirrors `usePhases`/`useTasks`) and three new `ApiClient.ts` functions (`getProjectMembers`/`addProjectMember`/`removeProjectMember`) calling PM01.3's endpoints, reusing `AppUserDto` — no new DTOs. Add-member is an avatar-grid picker (click to add immediately); remove is per-row with no self-protection, matching the backend's own lack of special-casing. Verified live against the real dev API/DB: existing pre-PM01 projects correctly show "No members yet.", a project created after PM01.2 shipped correctly shows its creator with zero manual setup, add/remove both update the panel immediately with no page reload, and `tsc --noEmit` passes clean.

**PM02.3 (ad-hoc addition, requested after PM02.1/.2 shipped):** tasks can now only be assigned to actual project members. Backend: `CreateTaskCommandValidator`/`UpdateTaskCommandValidator` gained async, DB-backed rules checking `AssignedUserId`/`SecondaryAssigneeId` against `IProjectRepository.IsMemberAsync` (new), which required switching `ValidationBehavior` from sync `Validate()` to `ValidateAsync()`. The update-side check only fires when the requested value actually *differs* from the task's current one — otherwise a task whose assignee was later removed from the project would become permanently un-editable, since the frontend always resends the current assignee on every save regardless of whether it changed. Frontend: `TaskForm`/`TaskDetailPanel`'s assignee pickers now source from a `members` prop (project roster) instead of the full `useUsers()` list — `BoardScreen` now owns `useProjectMembers` alongside `useUsers` and threads `members` down, since the roster crossed from single- to multi-consumer. Read-only assignee display still resolves from the full user list, so a since-removed assignee still shows correctly outside edit mode. `MembersPanel` gained an `onMembershipChanged` callback so `BoardScreen`'s independent `useProjectMembers` instance doesn't go stale relative to the Members panel's own. Verified against an isolated throwaway container + temporary API/Vite instances (not the real dev DB): the full matrix of create/update × member/non-member × changed/unchanged assignee, plus the staleness fix, all confirmed live.

---

### PRI01
**Backend: Voting & weighted scoring** — MVP-Required (Sprint 5) — ✅ Done (signed off 2026-08-28)
New `TaskVote` entity (unique per task+user, 1–10 scale) and `Project.CriticalityScore` (1–10, admin-settable) + EF migration. Weighted score computation: `ceiling((PriorityNumeric + AvgUserVote + CriticalityScore) / 3)`, where `PriorityNumeric` maps the existing `Priority` enum onto the same 1–10 scale (Low=3, Medium=7, High=9, Critical=10) — every averaging/rounding step in the chain always rounds up, never down. Cast/update-vote endpoint restricted to `ProjectMember`s (PM01), one editable vote per user per task, attributed to the current authenticated user (AUTH02). Admin-settable `Project.CriticalityScore`. Applies uniformly to `DailyTask`-scope tasks too — confirmed with the user, no scope-gating like Phase/Epic. See [CompletedTickets.md](CompletedTickets.md#pri01-tickets--backend-voting--weighted-scoring) for the full ticket-by-ticket history (PRI01.1–PRI01.4, 11 points).

---

### PRI02
**Frontend: Voting UI** — MVP-Required (Sprint 5) — ✅ Done (signed off 2026-08-28)
1–10 voting control wired into the shared `TaskDetailPanel`, showing the current user's vote plus the aggregate — automatically usable from all three surfaces that open it (Kanban, List view, and `TodoList`), since it's one shared component instance. Computed weighted score displayed on `TaskCard` (bottom-right corner, since DM02.7's epic badge already claims the top-right), `ListView` rows, and `TodoList` rows (PRI02.4, added since `DailyTask` tasks are weighted the same as everything else). Revised 2026-08-26 to correct file paths that drifted since first drafted (`TaskCard` moved to `@taskmanager/ui`; `ListView`/`KanbanBoard` moved into their own folders) and to add `TodoList` coverage that didn't exist when this epic was first scoped. See [CompletedTickets.md](CompletedTickets.md#pri02-tickets--frontend-voting-ui) for the full ticket-by-ticket history (PRI02.1–PRI02.4, 11 points).

**PRI02.5 (ad-hoc addition, requested 2026-08-27 after PRI01/PRI02 were built and verified):** the user asked where a project's `CriticalityScore` weight gets set and it turned out no PRI02 ticket ever built a frontend control for PRI01.4's admin endpoint (`PUT /api/projects/{id}/criticality-score`) — the only path to change it was a direct API call. First shipped as a small control embedded in `MembersPanel`; the user then redirected to a dedicated Admin-section screen instead ("we have an Admin Page... add a new page view off admin that lists all the projects"), so the `MembersPanel` version was fully reverted and rebuilt as `ProjectCriticalityScreen` (`frontend/packages/web/src/screens/admin/ProjectCriticalityScreen/`) — a table of every project with an inline 1–10 input per row, wired into `AdminShell.tsx`'s nav, inheriting the Admin section's existing admin-only gating for free. Originally committed on blur; the user reported it "doesn't seem to save," so it was rebuilt with an explicit **Save** button per row instead. Explicitly still interim: there is no Project Settings page yet, so this screen is a stand-in to be migrated/retired once one exists — tracked as a Post-MVP Backlog item, see [ROADMAP.md](ROADMAP.md#post-mvp-backlog) ("Project Settings page (per-project, admin-scoped)"). See [CompletedTickets.md](CompletedTickets.md#pri02-tickets--frontend-voting-ui) for the ticket (PRI02.5, 3 points).

---

### PRI03
**Default sorting by weighted score** — MVP-Required (Sprint 5) — ✅ Done (signed off 2026-08-28)
Board/List/`TodoList` views default-sort tasks by weighted score descending. Project selector default-sorts projects by `CriticalityScore` descending. Full sorting/filtering *controls* (as opposed to just default order) are logged as a post-MVP backlog item — see [ROADMAP.md](ROADMAP.md#post-mvp-backlog). See [CompletedTickets.md](CompletedTickets.md#pri03-tickets--default-sorting) for the full ticket-by-ticket history (PRI03.1–PRI03.2, 4 points).

---

### U02
**Implement FAB quick-capture modal** — Post-deployment
Floating Action Button on the board screen that opens a bottom sheet modal. Supports task title, project selector, priority picker, single-tap assignee, and optional due date. On submit, calls `createTask()` from `@taskmanager/shared` directly and closes instantly — the board refetches. See [TICKETS.md](TICKETS.md#u02-tickets) for tickets.

---

### M01
**Scaffold MCP server**
`mcp/` directory with Node.js/TypeScript MCP server using `@modelcontextprotocol/sdk`. Configures via `TASKMANAGER_API_URL` env var. Includes Dockerfile.

---

### M02
**Implement core MCP tools**
`list_projects`, `get_project`, `create_task`, `update_task`, `transition_task`, `list_tasks` — all proxying to the .NET REST API.

---

### M03
**Implement agent plan tracking MCP tools**
`create_plan`, `log_step`, `complete_step`, `get_plan_status` — structured plan tracking with `ExternalMetadata` stamps (agent ID, session ID, step index).

---

### M04
**Containerize MCP server & wire into docker-compose**
Add MCP server to `docker-compose.yml`. Internal Docker network to backend. Expose via Tailscale/local subnet.

---

### D01–D03
**Monolithic production Docker image (build stages, runtime base, assembly & verification)** — ✅ Done (signed off 2026-08-31)
Redefined per user direction (2026-08-30) from three separate images orchestrated via docker-compose into a single monolithic container — Postgres, the API, and the built frontend all run together under one process supervisor (s6-overlay), for simpler operation on a single-host Unraid deployment. Built as one root-level `Dockerfile`: `build-api` (`dotnet publish` `TaskManager.API` in Release via the .NET 10 SDK image) and `build-web` (`npm ci` + `npm run build --workspace=packages/web`) feed into a `final` stage — Postgres 16 + .NET 10 ASP.NET runtime + Nginx on Alpine, supervised by s6-overlay v3, running as a non-root `appuser`. The pre-existing standalone `backend/Dockerfile` (API-only, found during exploration) was deleted, superseded by `build-api`. Nginx serves the frontend on port 8081 and proxies both `/api/*` and `/avatars/*` to the API on `localhost:8080`; the API is also separately exposed on its own port 8080 for direct future consumers (the MCP server in Phase 11, or webhook integrations). Postgres data and avatar uploads persist on volumes intended for `/mnt/user/appdata/taskmanager/` on Unraid. `packages/mobile`'s Capacitor stub stayed fully out of scope (tracked post-MVP as MOB01).

Two real, non-obvious bugs were found and fixed during implementation: (1) `VITE_API_URL` had to be set to an **empty string** at build time rather than the originally-planned `/api` — `ApiClient.ts`'s `BASE_URL` is used as axios's `baseURL` *and* is prepended directly to endpoint paths that already embed `/api/...` themselves (and to `/avatars/...` asset paths), so `/api` would have doubled to `/api/api/projects`; an empty string resolves everything relative to the page's own origin correctly. (2) `docker/s6-overlay/scripts/api-run.sh` originally didn't `cd` into `/app/api` before exec'ing `dotnet TaskManager.API.dll` by absolute path — ASP.NET Core's `ContentRootPath` comes from the process's working directory, not the assembly's location, so `appsettings.json` silently failed to load, `Jwt:Issuer`/`Jwt:Audience` were never bound, minted JWTs silently omitted their `iss`/`aud` claims, and every authenticated request failed with 401 forever, even immediately after a successful login — fixed by adding `cd /app/api` before the exec.

Also found: `with-contenv` is required in every s6 service wrapper because s6-overlay v3 does **not** auto-inherit Docker's env vars into supervised services — a non-obvious behavior that would otherwise leave `Jwt__Secret`/connection strings invisible to the API process. The `postgresql16` Alpine package creates its own system `postgres` user and locks `/var/lib/postgresql` down to `drwxr-x---`, blocking the non-root `appuser` from even traversing into it — required an explicit `chown -R appuser:appuser /var/lib/postgresql` in the Dockerfile. The bundled Postgres uses a distinct internal connection string/password rather than PREP.4's literal dev-fallback string, so PREP.4's Production startup guard doesn't misfire even though `localhost` genuinely is correct here (Postgres is in the same container).

Verified live end-to-end (not simulated): `docker build` succeeds; s6 brings up Postgres → API → Nginx in the correct order; a full browser walkthrough completed the AUTH02 first-run admin setup, logged in, and created a project; the API responded identically whether hit directly on 8080 or proxied through 8081; stopping/removing the container and re-running against the same volumes preserved all data with `postgres-init` correctly skipping re-initialization; omitting `Jwt__Secret` correctly failed fast with PREP.4's existing error. Closes out Sprint 7 in full — this also completes [ROADMAP.md](ROADMAP.md#phase-9-containerization--initial-deployment) Phase 9. See [CompletedTickets.md](CompletedTickets.md#d01d03-tickets--monolithic-production-docker-image) for the full ticket-by-ticket history (D01.1–D01.2, D02.1–D02.2, D03.1–D03.2).

---

## 📌 Status Key

| Symbol | Meaning |
|---|---|
| 🔲 Pending | Not started |
| 🔄 In Progress | Actively being built |
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
- **B06** — Background Automation Engine (async webhook queue processing)
- **Home Assistant Integration** — Native HA integration beyond webhooks
- **Configurable Logging** — Runtime log level/sink settings in admin UI
- **Custom Webhook Builder** — Define outbound webhooks dynamically from admin UI
- **In-app "needs your vote" notifications** — Bell icon/badge surfacing tasks the current user hasn't yet voted on (Phase 7's voting system). In-app only, not push/email/SMS. Depends on Phase 7 shipping first. See [ROADMAP.md](ROADMAP.md#post-mvp-backlog) for the full writeup.
- **Email Notifications** — Assignee alerts on task create/update/complete
- **SMS Notifications** — High-priority task assignment alerts
- **BFF (Backend for Frontend)** — Per-client optimized API layer once web + mobile diverge meaningfully
