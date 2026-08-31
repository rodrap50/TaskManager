# Tickets — HomeAssistant Project Tracker

> Active granular tickets for pending epics.
> Epics live in [TASKS.md](TASKS.md) · Completed tickets archived in [CompletedTickets.md](CompletedTickets.md).

---

## Table of Contents

**🐛 Bug Tickets** *(all ✅ Done, archived)*
- *Sprint 1 Bug Tickets — App-Breaking — BUG-01, BUG-04, BUG-05, BUG-06 — archived to [CompletedTickets.md](CompletedTickets.md#sprint-1-bug-tickets--app-breaking)*
- *Sprint 3 Bug Tickets — UX Polish (Pre-Deployment) — BUG-02, BUG-03, BUG-07 — archived to [CompletedTickets.md](CompletedTickets.md#sprint-3-bug-tickets--ux-polish-pre-deployment)*

**🎨 Sprint 1b — UI Visual Overhaul & Board Build**
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

**🔐 Sprint 4 — Identity & Access (MVP-Required)**
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
| ✅ Done | Complete |
| ⏸ Deferred | Moved to post-MVP |

---

## AUTH03 Tickets — Admin: API Token Management UI

**Epic:** Admin: API token management UI — ⏸ Deferred (Post-MVP)

> Pushed post-MVP per user direction — not required for initial deployment. The AUTH01 backend (generate/list/revoke `ApiToken`s) already shipped and works fine via direct API calls; this ticket only adds the admin-panel UI on top of it. Absorbs the token-management half of A03 (Webhook & Integration Manager) — see [TASKS.md](TASKS.md#a03).

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

## U03 Tickets

**Epic:** Add gesture-driven task interactions — ⏸ Deferred (Mobile App Phase)

Gesture interactions (swipe-to-complete, swipe-to-assign) are touch-native features that belong in `packages/mobile`. Deferred until the mobile Capacitor build is activated post-MVP.

---
