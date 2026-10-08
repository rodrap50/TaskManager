# Completed Tickets — HomeAssistant Project Tracker

> Archive of all finished implementation tickets.
> Active tickets live in [TICKETS.md](TICKETS.md). Epics live in [TASKS.md](TASKS.md).

---

## U01 Tickets

**Epic:** Build Kanban board & list dashboard — ✅ Done

| Ticket | Title |
|---|---|
| U01.1 | Expand local SQLite schema for tasks, phases & users |
| U01.2 | Build typed API client (`ApiClient.ts`) |
| U01.3 | Build data hooks (`useProjects`, `useTasks`, `usePhases`, `useUsers`) |
| U01.4 | Build app shell & project selector screen |
| U01.5 | Build shared task card & priority/status components |
| U01.6 | Build Kanban board view |
| U01.7 | Build list view |
| U01.8 | Wire view toggle, full data flow, and sync bar |

---

## ARCH01 Tickets

**Epic:** npm workspace monorepo restructure — ✅ Done

| Ticket | Title |
|---|---|
| ARCH01.1 | Create workspace root `package.json` with `packages/*` workspaces |
| ARCH01.2 | Scaffold `@taskmanager/shared` — axios API client + all TypeScript DTOs |
| ARCH01.3 | Scaffold `@taskmanager/ui` — TaskCard, PriorityBadge, StatusIndicator, AssigneeAvatar |
| ARCH01.4 | Scaffold `@taskmanager/web` — clean always-online Vite app (no SQLite/Capacitor) |
| ARCH01.5 | Scaffold `@taskmanager/mobile` — preserve all Capacitor/SQLite/sync code, deferred |
| ARCH01.6 | Verify web dev server starts and renders with no WASM errors |

### ARCH01 Context

**Why:** The original single Vite app crashed on launch because `jeep-sqlite` attempted to load a WASM binary, but Vite's dev server served the SPA fallback HTML instead (MIME type `text/html` vs `application/wasm`). Multiple plugin-level fixes were attempted and failed. The root decision was to stop treating the dev environment as the problem and instead separate concerns architecturally.

**Result:** `packages/web` is always-online — it calls the .NET API directly via axios, has zero SQLite or Capacitor imports, and boots cleanly. `packages/mobile` preserves all offline-first work intact for the post-MVP mobile phase. Both packages share `@taskmanager/shared` (API client + DTOs) and `@taskmanager/ui` (visual components).

**Workspace layout:**
```
frontend/
  package.json           ← workspace root (npm workspaces: ["packages/*"])
  packages/
    shared/              ← @taskmanager/shared: ApiClient.ts, all DTOs, axios instance
    ui/                  ← @taskmanager/ui: TaskCard, PriorityBadge, StatusIndicator, AssigneeAvatar
    web/                 ← @taskmanager/web: always-online WebUI, direct API calls
    mobile/              ← @taskmanager/mobile: Capacitor + jeep-sqlite + offline-first (post-MVP)
```

**Dev server:** `npm run dev --workspace=packages/web` (configured in `.claude/launch.json`)

---

## Sprint 1 Bug Tickets — App-Breaking

These bugs prevent the app from functioning at all. Sprint 1 is not closeable until all are resolved.

| Ticket | Title | Status | Severity |
|---|---|---|---|
| BUG-01 | Backend missing `JsonStringEnumConverter` — enum strings fail to deserialize | ✅ Done | 🔴 Critical |
| BUG-04 | API base URL hardcoded to wrong port — frontend can't reach backend | ✅ Done | 🔴 Critical |
| BUG-05 | `CreateProjectModal` silently swallows API errors — user sees nothing on failure | ✅ Done | 🔴 Critical |
| BUG-06 | API load errors never displayed — screens show empty state instead of error | ✅ Done | 🟠 High |

---

### BUG-01 — Backend missing JsonStringEnumConverter

**File:** `backend/TaskManager.API/Program.cs`

**Problem:** `AddControllers()` has no JSON options. `System.Text.Json` only deserializes enums by integer ordinal by default. The frontend sends all enum values as strings (`scope: "Project"`, `priority: "Medium"`, `newStatus: "InProgress"`). Every write endpoint fails: `CreateProject`, `CreateTask`, `UpdateTask`, `TransitionTask`.

**Fix:** Chain `.AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()))` onto `AddControllers()` in `Program.cs`.

---

### BUG-04 — API base URL hardcoded to wrong port

**Files:** `frontend/packages/shared/src/ApiClient.ts`, `frontend/packages/web/` (missing `.env.development`)

**Problem:** `ApiClient.ts` falls back to `http://localhost:5000` when `VITE_API_URL` is not set. The backend dev profile runs on `http://localhost:5062` (`launchSettings.json`). No `.env.development` file exists in `packages/web/`. Result: every API call gets a connection refused error in dev.

**Fix:**
1. Create `frontend/packages/web/.env.development` with `VITE_API_URL=http://localhost:5062`
2. Create `frontend/packages/web/.env.example` documenting the variable for future reference

---

### BUG-05 — CreateProjectModal silently swallows API errors ✅

**File:** `frontend/packages/web/src/screens/ProjectSelectorScreen.tsx` → `CreateProjectModal`

**Problem:** `handleSubmit` uses `try { ... } finally { setSubmitting(false) }` with no `catch`. When `createProject()` throws (connection refused, 400, 500, etc.), the error propagates to `void handleSubmit()` and is silently dropped. The button flashes "Creating…" then reverts to "Create" — the user has no idea what went wrong.

**Fix:** Added a `catch` block that stores the error in state (`const [error, setError] = useState<string | null>(null)`, cleared at the start of each submit) and renders it as a red inline message between the colour swatches and the action buttons. Verified in-browser: happy path (create succeeds, modal closes), failure path (forced network error → inline "Network Error" message, button resets to "Create"), and retry path (error clears and submission succeeds once the fault is removed).

---

### BUG-06 — API load errors never displayed in screens ✅

**Files:** `frontend/packages/web/src/screens/ProjectSelectorScreen.tsx`, `frontend/packages/web/src/screens/BoardScreen.tsx`

**Problem:** `useProjects`, `useTasks`, `usePhases`, and `useUsers` all return an `error` field, but neither `ProjectSelectorScreen` nor `BoardScreen` renders it. A failed load (backend down, CORS, network error) results in a permanently empty screen with no feedback — identical to "no data" state.

**Fix:** `ProjectSelectorScreen` now destructures `error` from `useProjects()` and renders "Failed to load — check the API is running" in place of the empty state when non-null. `BoardScreen` does the same for `useTasks`'s `error`. Verified in-browser by forcing each GET call to fail: the error message renders correctly and clears on a clean reload with no regression to the normal loading/empty/data states.

---

## Sprint 3 Bug Tickets — UX Polish (Pre-Deployment)

Lower-severity bugs to address in Sprint 3 before deployment.

| Ticket | Title | Status | Severity |
|---|---|---|---|
| BUG-02 | `TaskDetailPanel` phase highlight stale after change | ✅ Done | 🟡 Minor UX |
| BUG-03 | `ListView` list rows stale after detail panel mutations | ✅ Done | 🟡 Minor UX |
| BUG-07 | Done/Cancelled tasks can only reopen to Backlog, but board doesn't show this | ✅ Done | 🟠 High |

---

### BUG-02 — TaskDetailPanel phase highlight stale after change ✅

**File:** `frontend/packages/web/src/features/board/ListView.tsx` → `TaskDetailPanel` (superseded — see fix)

**Problem:** The phase section checks `task.phaseId` (from the original prop) to determine which phase button is highlighted. `phaseId` is never stored in local state, so after clicking a new phase, the old phase button remains highlighted.

**Fix — shipped as part of DS05.2:** the old embedded panel in `ListView.tsx` was superseded by the new standalone `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx`, which uses `const [phaseId, setPhaseId] = useState(task.phaseId)` and reads that local state (not `task.phaseId`) for the active phase highlight. Verified live: created two temporary phases via a direct API call, clicked between them, and confirmed the active pill highlight updated instantly (computed class list showed only the clicked pill carrying `bg-primary-800`); temporary phases removed afterward. Note: at the time this ticket was written, `ListView.tsx` was believed to still have its own embedded `TaskDetailPanel` carrying the same stale-highlight bug, pending DS06.2 to consolidate it onto this component. DS06.2 later confirmed that premise was already stale — `ListView` has never rendered its own panel; `BoardScreen` has fed both Kanban and List view into this same shared component since DS05 shipped, so this fix already covered the List surface with no separate consolidation step needed. See [DS06.2 in CompletedTickets.md](CompletedTickets.md#ds06-tickets--restyle-list-view). Full implementation notes archived under [DS05.2 in CompletedTickets.md](CompletedTickets.md#ds05-tickets).

---

### BUG-03 — ListView list rows stale after detail panel mutations ✅

**File:** `frontend/packages/web/src/features/board/ListView.tsx` → `ListView` (superseded — see fix)

**Problem:** After changing status/priority/assignee in the `TaskDetailPanel` and closing it, the list rows still show the pre-mutation values. `ListView` has no access to `refetch` and never calls it on panel close.

**Fix — shipped as part of DS05.4:** the new standalone `TaskDetailPanel` (`frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx`) accepts a `refetch` prop and calls it from its own `handleClose` (used by the backdrop click, the header close button, and Escape). `BoardScreen` threads `refetch={refetchTasks}` (from `useTasks`) through to the panel. Verified live: edited a task's title/due-date, closed the panel, and confirmed the Kanban card showed the updated values immediately without a manual page reload (confirmed via a fresh `GET /api/tasks/project/...` network request firing on close). Note: at the time this ticket was written, `ListView.tsx` was believed to still have its own embedded `TaskDetailPanel` carrying the same stale-rows bug, pending DS06.2 to consolidate it onto this component. DS06.2 later confirmed that premise was already stale — `ListView` has never rendered its own panel; `BoardScreen` has fed both Kanban and List view into this same shared component since DS05 shipped, so this fix already covered the List surface with no separate consolidation step needed. See [DS06.2 in CompletedTickets.md](CompletedTickets.md#ds06-tickets--restyle-list-view). Full implementation notes archived under [DS05.4 in CompletedTickets.md](CompletedTickets.md#ds05-tickets).

---

### BUG-07 — Done/Cancelled tasks can only reopen to Backlog, but board doesn't show this ✅

**File:** `backend/TaskManager.Domain/Entities/TaskItem.cs` (`Transition` method) — domain rule is correct and stays as-is; `frontend/packages/web/src/features/board/KanbanBoard.tsx` — visual fix implemented here

**Problem:** `TaskItem.Transition()` intentionally only allows a `Done`/`Cancelled` task to be reopened to `Backlog` — any other target status throws `InvalidOperationException`. This is correct domain behaviour (confirmed with the user), but the Kanban board's drag-and-drop (shipped as part of DS03.2) didn't reflect the restriction: a card dragged out of the Done column onto InProgress/Todo/InReview was allowed to drop, the API call failed, and the card silently reverted with no explanation.

**Fix — shipped as part of DS03.4:** while dragging a card whose current status is `Done` or `Cancelled`, every column except `Backlog` is now treated as a locked-out drop target (`draggedTask`/`restrictedToBacklog`/`isLockedOut(status)` logic in `KanbanBoard.tsx`). `onDragOver` skips `e.preventDefault()` for locked-out columns so the drop is rejected at the browser level before any API call is made, and those columns render a dimmed `columnInvalid` style (`opacity-40 cursor-not-allowed`) only for the duration of that specific drag. Verified live: dragging a `Done` task onto `InProgress` was rejected (status unchanged in the DB, confirmed via direct API check); the same task dropped onto `Backlog` succeeded. Full implementation notes archived under [DS03.4 in CompletedTickets.md](CompletedTickets.md#ds03-tickets).

---

## DS01 Tickets — Design System Foundation

**Epic:** Design system: tokens, Tailwind config, CSS vars — ✅ Done (Sprint 1b)

| Ticket | Title | Status |
|---|---|---|
| DS01.1 | Extend `tailwind.config.ts` with zinc/crimson palette | ✅ Done |
| DS01.2 | Define CSS custom properties for dark and light modes | ✅ Done |
| DS01.3 | Configure Tailwind dark mode (`class` strategy) and verify token swap | ✅ Done |

---

### DS01.1 — Extend tailwind.config.ts with zinc/crimson palette ✅

**File:** `frontend/packages/web/src/index.css` (see note below — not `tailwind.config.ts`)

**Goal:** Add a named custom palette so all components use semantic tokens, not raw Tailwind colors.

**Note on file deviation:** The project runs Tailwind v4 (CSS-first config, `@import "tailwindcss"` in `index.css`, no `tailwind.config.ts` exists). Tailwind v4's idiomatic equivalent of extending the JS config is a `@theme { }` block in CSS — declaring `--color-surface`, `--color-primary-800`, etc. there both registers the token *and* is the CSS variable itself, so utilities like `bg-surface` are auto-generated. Introducing a legacy `tailwind.config.ts` alongside the v4 CSS-first setup would fight the framework's own architecture, so the palette was added to `index.css` instead.

**Acceptance criteria:**
- `colors.surface` scale: `DEFAULT` (page ground), `raised` (cards), `overlay` (modals/dropdowns) — ✅ `bg-surface`, `bg-surface-raised`, `bg-surface-overlay`
- `colors.border` scale: `DEFAULT` (primary border), `subtle` (dividers) — ✅ `border-border`, `border-border-subtle`
- `colors.primary` scale: 900 / 800 / 700 mapped to crimson range (`#7F1D1D` / `#991B1B` / `#B91C1C`) — ✅ `bg-primary-900/800/700`
- `colors.text` scale: `DEFAULT`, `muted`, `inverted` — ✅ `text-text`, `text-text-muted`, `text-text-inverted`
- All values reference CSS variables (e.g. `var(--color-surface)`) so dark/light swap at the CSS layer, not the class layer — ✅ each token is a plain CSS custom property under `@theme`, so DS01.2 can override values per mode without touching any component class

Verified: each of the 11 generated utility classes was smoke-tested individually in-browser (temporary probe elements, removed after) and all resolved to their exact intended hex/rgba values. Dark-first values used as the initial defaults per the design system direction; DS01.2 will add the explicit `:root`/`.light` mode split.

---

### DS01.2 — Define CSS custom properties for dark and light modes ✅

**File:** `frontend/packages/web/src/index.css`

**Goal:** A single CSS layer that defines the full palette for both modes.

**Note:** Tailwind v4 compiles the `@theme` block from DS01.1 onto `:root`, so those values already ARE the dark-mode `:root` declaration (satisfying "dark-first `:root` block" without a redundant hand-written duplicate — documented with a comment in `index.css`). Added a `.light` class block that overrides the mode-dependent tokens only; `--color-primary-*` is intentionally omitted from `.light` since primary stays constant across modes.

**Acceptance criteria:**
- `:root` block defines dark-mode values (dark-first) — ✅ via the existing `@theme` block (see note)
- `.light` class block overrides to light-mode values — ✅
- Variables: `--color-surface`, `--color-surface-raised`, `--color-surface-overlay`, `--color-border`, `--color-border-subtle`, `--color-primary-900`, `--color-primary-800`, `--color-primary-700`, `--color-text`, `--color-text-muted`, `--color-text-inverted` — ✅ all present
- Dark ground: `zinc-950` (`#09090b`); cards: `zinc-900`; overlay: `zinc-800` — ✅
- Light ground: `zinc-100`; cards: `white`; overlay: `zinc-50` — ✅
- Border dark: `zinc-700` at 50% opacity; light: `zinc-200` — ✅
- Primary unchanged across modes — ✅ verified `bg-primary-900/800/700` resolve identically with and without `.light`

**Values inferred beyond the ticket's explicit list** (ticket didn't specify these, chosen for consistency with the zinc scale already in use): light `border-subtle` = `zinc-100` (`#f4f4f5`, a barely-visible divider close to ground, mirroring dark mode's subtle-blends-into-background pattern); light `text-muted` = `zinc-500` (`#71717a`, legible gray on light backgrounds); `text-inverted` = `#ffffff` in both modes (used for text on solid primary-colored surfaces like buttons, which need light text regardless of theme).

Verified in-browser: temporary probe elements for all 11 tokens, read before/after toggling a `.light` class on `<html>` — dark values matched DS01.1's verified set, light values matched the table above, and primary was confirmed identical in both states. Probes removed after verification; no console errors, no regression.

---

### DS01.3 — Configure Tailwind dark mode and verify token swap ✅

**File:** `frontend/packages/web/src/App.tsx` (no `tailwind.config.ts` — see DS01.1's note; this design system swaps via the `.light` class + CSS variables, not Tailwind's `dark:` variant, so there is no `darkMode` config setting to make)

**Goal:** Wire the dark/light toggle so the design system is verifiably working before any screen work begins.

**Acceptance criteria:**
- Tailwind `darkMode: 'class'` set in config — N/A for this project's architecture (see file note above); the equivalent mechanism is `AppShell` toggling the `.light` class on `document.documentElement`, which is what DS01.2's CSS actually keys off
- `App.tsx` reads a `theme` value from `localStorage` (`'dark'` default) and applies the class to `<html>` — ✅ `loadTheme()` + `useEffect` in `AppShell`
- A temporary toggle button (can be removed later) lets the developer flip modes in-browser — ✅ header button, clearly labeled as temporary (to be replaced by the real Settings screen)
- Spot-check: page ground color changes correctly between modes — ✅ root container uses `bg-surface`; rest of the shell chrome (header/nav) intentionally left on its original raw Tailwind classes for DS02.1 to restyle, so this ticket only proves the token swap works end-to-end

Verified in-browser: default load is dark (`bg-surface` → `rgb(9,9,11)`, `localStorage['tm:theme']` seeded to `'dark'`); clicking the toggle flips to light (`rgb(244,244,245)`) and persists across a full page reload; toggling back to dark confirmed clean. No console errors.

---

## DS02 Tickets — App Shell & ProjectSelectorScreen Restyle

**Epic:** Restyle app shell & ProjectSelectorScreen — ✅ Done (Sprint 1b)

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| DS02.1 | Restyle app shell and page ground | ✅ Done | DS01 |
| DS02.2 | Restyle ProjectSelectorScreen project cards | ✅ Done | DS02.1 |
| DS02.3 | Restyle Create Project modal | ✅ Done | DS02.1 |

---

### DS02.1 — Restyle app shell and page ground ✅

**File:** `frontend/packages/web/src/App/App.tsx` (moved into its own folder — see note)

**Note — file organization convention (applies going forward to all DS02+ work):** Per team direction, each screen/component now gets its own subdirectory with a colocated styles file and a barrel `index.ts`, e.g. `App/{App.tsx, App.styles.ts, index.ts}`. `App.styles.ts` exports a `useAppStyles()` hook returning an object of `className` strings (e.g. `styles.header`, `styles.navTabActive`) which the component destructures and applies — updated from an earlier plain-constants draft per follow-up direction. `index.ts` re-exports the default so external imports stay short (`from './App'` still resolves — no callers needed updating). This is being applied incrementally as each file is touched for its ticket, not retroactively across the whole codebase.

**Acceptance criteria:**
- Page ground uses `bg-surface` token — ✅
- Any top nav / chrome uses `bg-surface-raised` with a `border-border` bottom border — ✅ header and `BottomNav`
- Typography uses `text-text` and `text-text-muted` tokens — ✅
- No raw Tailwind color classes remain in the shell — ✅ removed all `gray-*`/`white`/`blue-*` classes from `App.tsx`; active nav tab now uses `text-primary-700`

Verified in-browser: computed styles confirmed header/nav → `rgb(24,24,27)` (surface-raised) and title → `rgb(250,250,250)` (text) in dark mode, flipping correctly in light mode; active nav tab renders crimson (`rgb(185,28,28)`, primary-700) unchanged across modes. Hit a Vite dev-server transform-cache issue after deleting the old flat `App.tsx` (stale resolved import path, 404s) — fixed by re-saving `main.tsx` to force re-resolution; confirmed clean afterward with no console errors.

**Follow-up refinement (user feedback after visual validation):** (1) added crimson-tinted shadows to header (`shadow-[0_4px_20px_-6px_rgba(153,27,27,0.35)]`) and bottom nav (mirrored upward) so the primary color reads more strongly, especially in dark mode. (2) Light mode's header was nearly indistinguishable from the page ground (`surface-raised` white vs. `surface` zinc-100 are very close in value, and the zinc-200 border was too subtle to compensate) — bumped light `--color-border` from zinc-200 to zinc-300 (`#d4d4d8`) for real definition, and `--color-border-subtle` from zinc-100 to zinc-200 accordingly. Note: any remaining "text disappears" in light mode on screens *other than* the shell (project cards, buttons) is because those screens still use raw Tailwind `dark:` classes keyed to OS `prefers-color-scheme`, independent of this app's `.light` toggle — that mismatch resolves naturally as DS02.2/DS02.3 migrate those screens onto the new tokens.

**Second follow-up (user requested a crimson-to-grey fade for more header contrast):** first tried a separate 3px gradient accent bar below the header rather than gradient-filling the header itself, to sidestep a text-legibility risk. User clarified they specifically wanted the header's own background to carry the fade, not a border/bar. Resolved the legibility concern by using a translucent crimson stop instead of a full-strength one: `bg-gradient-to-r from-primary-900/25 to-surface-raised` (removed `headerAccent` and its separate `<div>`). Verified via composited-contrast math: the crimson-tinted left edge composites to `rgb(38,14,15)` (dark maroon) in dark mode and `rgb(215,190,191)` (muted dusty pink) in light mode — both comfortably above WCAG AA contrast against the header's white/near-white or near-black text tokens. No console errors.

**Third follow-up — design pivot (user liked dark mode, disliked light mode, considered dropping the header entirely):** Rather than keep tuning one gradient formula to work in both modes, the header bar concept itself was replaced. After a design discussion (6 options: 2 safe, 2 middle, 2 leading-edge — see conversation), the user chose to combine both leading-edge ideas: a chrome-less shell with a floating "reactive pill" instead of a full-width header, carrying both an ambient breathing glow *and* a cursor-following spotlight glow.

**What changed:** the `<header>` element is gone entirely — `App.styles.ts` lost `header`/`headerLeft`/`backButton`/`title`/`themeToggle`. New component: `App/FloatingPill/{FloatingPill.tsx, FloatingPill.styles.ts, index.ts}` — a `position: fixed` glass pill (`bg-surface-raised/90 backdrop-blur-md`, rounded-full) anchored top-right, containing: a small crimson brand dot (replaces the "TaskManager" text — no persistent header means no room for a full wordmark), the back button (conditional, same logic as before), and the theme toggle (icon-only now). It floats *over* content — `main` is no longer pushed down by header height. A `@keyframes pillGlow` rule in `index.css` drives a continuous slow box-shadow pulse (crimson, via `color-mix()` against `--color-primary-700` so it's token-driven); on hover, an inner radial-gradient layer tracks the cursor position (via `onMouseMove` + `getBoundingClientRect()`) and the ambient pulse pauses so the two effects don't fight. Both respect `prefers-reduced-motion`: the ambient animation is disabled with a static-glow fallback, and the spotlight never activates the mouse-tracking handler for that group.

**Acceptance criteria — reinterpreted for the new design** (original criteria assumed a conventional header bar; superseded by the pivot above):
- Crimson presence — ✅ via the pill's ambient/spotlight glow, both derived from `--color-primary-700`
- Chrome uses design tokens — ✅ `bg-surface-raised/90`, `border-border`, `text-text-muted`/`text-text`
- No raw Tailwind color classes — ✅
- Both modes verified — ✅ (see below)

Verified in-browser: pill renders `position: fixed`, `top: 16px`, `right: 16px`; ambient glow animation confirmed (`animationName: pillGlow`, `animationDuration: 5s`); hovering over the pill paused the ambient animation and activated the spotlight (`opacity: 1`, gradient position tracked real cursor movement — confirmed at two different hover points, 64.79% → 23.94%); theme toggle still flips `.light` correctly; back button appears only when a project is selected and correctly navigates back; no console errors throughout. Caught and fixed one bug during implementation: an accidental `relative` class was overriding the pill's `fixed` positioning in the compiled stylesheet (removed — `fixed` alone already establishes the positioning context the spotlight child needs).

**Fifth follow-up — Lucide icons:** replaced all emoji icons in the shell with `lucide-react` (installed in `packages/web`) — `ArrowLeft` (back button) and `Moon`/`Sun` (theme toggle) in `FloatingPill`; `Kanban`/`Folder`/`Settings` in `FloatingNav`. Icons inherit color via `currentColor` (Lucide's default), so they pick up the existing `text-text-muted`/`hover:text-text`/`text-primary-700` token classes automatically — no color props needed. Sized via `className` (`h-3.5 w-3.5` in the pill, `h-5 w-5` in the nav) rather than the old emoji `text-lg`/`text-sm` font-size approach. Verified in-browser: all 5 icons render as real `<svg class="lucide lucide-*">` elements, back button and theme toggle still function correctly, no console errors. Other emoji icons elsewhere in the app (ProjectSelectorScreen's empty state, BoardScreen's empty state) are untouched for now — will migrate to Lucide when DS02.2/DS03 touch those screens.

**Fourth follow-up (footer pill + content spacing):** User requested the same floating-pill treatment for the bottom nav (renamed `BottomNav` → `FloatingNav`, own folder `App/FloatingNav/{FloatingNav.tsx, FloatingNav.styles.ts, index.ts}`), specifically `w-[60vw] max-w-md`, centered (`left-1/2` + native CSS `translate: -50%`, not `transform`), `justify-between` tabs (dropped `flex-1` in favor of natural spacing at the wider pill width) for generous gaps between the 3 tabs. Reuses the same `pillGlow` ambient animation as the header pill for a consistent "same pill feel." Also added `pt-20 pb-20` to `main` so content clears both floating pills instead of sitting underneath them.

Verified in-browser: pill is `position: fixed`, `bottom: 16px`, centered (`rect.left=416, rect.right=864` on a 1280px viewport — exact center), width correctly capped at `max-w-md` (448px) since 60vw (768px) exceeds it on this screen size — confirms the "not over-stretched on large screens" requirement; `main` padding confirmed 80px top/bottom; ambient glow animating; theme toggle and back-button navigation still work; no console errors. One verification caveat: `getComputedStyle().color` reported the wrong (swapped) colors for the active/inactive tab highlighting on repeated queries against the same long-lived DOM nodes, but triangulated via React's fiber tree (`active` prop correctly `"board"`) and `element.matches('.text-primary-700')` (correctly `true` on the Board tab) that the actual code and DOM state are correct — likely a rendering-cache quirk in this automated browser session (possibly related to the `backdrop-blur` + animation combination), not a real bug. Flagged for the user to confirm visually since screenshot capture wasn't available this session.

---

### DS02.2 — Restyle ProjectSelectorScreen project cards ✅

**File:** `frontend/packages/web/src/screens/ProjectSelectorScreen/` (moved into its own directory per the DS02.1 file-organization convention — `ProjectSelectorScreen.tsx`/`.styles.ts`/`index.ts` plus child `ProjectCard/` and `CreateProjectModal/` subdirectories)

**Acceptance criteria:**
- Each project renders as a `bg-surface-raised border border-border rounded-lg` card with a subtle shadow (`shadow-md`) — the "floating" feel — ✅ (rounding later bumped to `rounded-xl`, shadow later re-tinted crimson — see follow-up)
- Cards have a left accent bar in `bg-primary-800` (or project color if stored) — ✅ `ProjectCard`'s `accentBar`, using `project.colorHex` with a `primary-800` fallback; replaces the earlier center-dot accent so the color signal isn't duplicated
- Hover state: border lightens or card lifts slightly (`hover:shadow-lg hover:-translate-y-px`) — ✅
- Empty state is clearly styled, not a raw text dump — ✅ dashed-border panel with a Lucide `FolderOpen` icon, replacing the old 📋 emoji + text
- Both dark and light modes verified — ✅

Also restyled in the same pass (same file, same ticket scope): screen header (`Projects` title + `New` button now `bg-primary-800`/Lucide `Plus`) and the load-error message (re-tokenized to `text-primary-700` instead of introducing a raw red).

**Follow-up (user tweaks + fixes, validated in-browser):** found and fixed a mangled Tailwind class from a manual edit (`border-border` + `bg-surface-raised` concatenated without a space, silently dropping both). Then: cards centered in a `max-w-2xl` column (screen header shares the same max-width so it aligns with the card edges) instead of stretching edge-to-edge; corner rounding bumped `rounded-lg` → `rounded-xl` (noticeably rounder, still short of pill); shadow changed from grey `shadow-md` to a **static** crimson-tinted shadow (`shadow-[0_4px_16px_-2px_rgba(185,28,28,0.25)]`, intensifying on hover) per the design system's chrome-vs-content pulse distinction (see [MISSION.md](MISSION.md#-design--style-decisions)). Verified via computed layout: card centered with equal left/right gaps in a 1280px viewport, `border-radius: 12px`, correct resting/hover box-shadow values, no `animate-*` class present.

---

### DS02.3 — Restyle Create Project modal ✅

**File:** `frontend/packages/web/src/screens/ProjectSelectorScreen/CreateProjectModal/`

**Acceptance criteria:**
- Modal uses `bg-surface-overlay border border-border` with `shadow-2xl` — ✅ (later layered with an optional crimson pulse — see follow-up)
- Feels elevated above the page — strong shadow, slight backdrop blur on the scrim — ✅ `backdrop-blur-sm` on the overlay
- Primary action button uses `bg-primary-800 hover:bg-primary-700 text-text-inverted` — ✅
- Error state (BUG-05 fix) styled in red inline below the field — ✅ re-tokenized onto the crimson scale (`border-primary-800/40 bg-primary-900/10 text-primary-700`) rather than raw `red-*`, keeping it in the same design language while still clearly reading as an alert
- Both dark and light modes verified — ✅ computed styles confirmed panel/input/label tokens swap correctly (e.g. `bg-surface-overlay` → `#27272a` dark / `#fafafa` light)

All other form chrome (labels, inputs, type-selector pills, color swatches, cancel button) migrated off raw `gray-*`/`blue-*` classes onto design tokens in the same pass.

**Follow-up 1 (color picker polish, user feedback):** the default swatch palette was Tailwind's stock 500-shade rainbow (blue/violet/emerald/amber/red/pink/teal/orange) — read as generic and the red/pink entries risked being confused with the brand's own crimson. Replaced with a curated 8-color set avoiding the red family entirely (`#6366f1` indigo, `#8b5cf6` violet, `#0ea5e9` sky, `#10b981` emerald, `#14b8a6` teal, `#f59e0b` amber, `#d946ef` fuchsia, `#64748b` slate). Swatches now render muted by default (`filter: saturate(0.7) brightness(0.85)` via a dedicated `.color-swatch` CSS class in `index.css`, scoped separately from the button's own selection ring so the ring color isn't muted too) and go full-saturation under `.light`. Fixed the reported "border collides with label" issue — `mt-3` added between the "Colour" label and the swatch row for breathing room the `ring-offset`/`scale` combo needed.

**Follow-up 2 (selection-indicator redesign, "weird spacing" bug):** the active swatch's `scale-125 ring-2 ring-offset-2` combination still looked visually off. Replaced with a dedicated `@keyframes swatchGlow` (crisp 2px crimson ring + close pulsing blur, ~1.8s, `index.css`) applied via `animate-[swatchGlow_...]`, dropping the scale/ring-offset entirely — verified via computed style: `transform: none`, correct two-layer `box-shadow`. Then extended the same treatment further ("for the fun of it"): the Type selector's active pill now uses the same `swatchGlow` animation (dropped its static border in favor of the animated ring, avoiding a doubled-border look), and the modal panel itself got a new `@keyframes panelGlow` (~3s) that layers the crimson pulse *underneath* the existing `shadow-2xl` elevation shadow rather than replacing it. All three (`pillGlow`, `swatchGlow`, `panelGlow`) fall back to a static crimson shadow/outline under `prefers-reduced-motion`. This extends pulse beyond the original "chrome pills only" rule — [MISSION.md](MISSION.md#-design--style-decisions) updated to formally sanction pulse as active/selected-state feedback (not just ambient chrome), documenting all three keyframes.

---

## DS03 Tickets

**Epic:** Build Board screen (Kanban view) — ✅ Done (Sprint 1b)

| Ticket | Title | Status |
|---|---|---|
| DS03.1 | Board screen shell + routing from project selection | ✅ Done |
| DS03.2 | Kanban columns (one per task status) | ✅ Done |
| DS03.3 | Task card component in new design system | ✅ Done |
| DS03.4 | Wire drag-drop status transitions | ✅ Done |

---

### DS03.1 — Board screen shell + routing from project selection ✅

**Files:** `frontend/packages/web/src/screens/BoardScreen/` (moved into its own directory + `ViewToggle/` child, per the DS02.1 file-organization convention)

**Note on scope drift since this ticket was written:** written before DS02.1's chrome-less pivot. Back navigation is already handled globally by `FloatingPill` (shown whenever a project is selected), so `BoardScreen` doesn't need its own back arrow. "+ New Task" is left out entirely rather than added as a dead button — DS04 (the modal it would open) doesn't exist yet.

**Acceptance criteria:**
- Selecting a project on `ProjectSelectorScreen` navigates to `BoardScreen` (pass `projectId`) — already worked via `ProjectContext`; unchanged
- Board screen has a top bar: back arrow → project list, project name, "+ New Task" button (opens DS04 modal) — **reinterpreted:** back arrow already global (see note); added the project name (new `useProjectDetail(projectId)` hook wrapping `getProjectById`, mirroring `usePhases`' shape) + the restyled `ViewToggle`; "+ New Task" deferred until DS04 exists
- Page ground and chrome use design tokens — header, `ViewToggle` (Lucide `LayoutGrid`/`List`, `bg-primary-800` active state), loading/error/empty states all migrated off raw `gray-*`/`blue-*`/`red-*`; empty state now a styled block with a Lucide `ListChecks` icon instead of a bare ✅ emoji
- `useTasks(projectId)` and `usePhases(projectId)` called; loading and error states rendered — unchanged logic, restyled (error text now `text-primary-700`, matching `ProjectSelectorScreen`'s convention)

Implemented and self-verified in-browser: clicked into a real project, confirmed project name rendered from the API (not a placeholder), `ViewToggle`'s active button computed to `rgb(153,27,27)` (`primary-800`), empty state renders with 1 icon + 2 lines of copy, no console errors from the current session (only stale history from earlier navigations).

**Follow-up (project-colored title accent, user-verified):** Added a `titleWrap` style class (`BoardScreen.styles.ts`) wrapping the project name title in `BoardScreen.tsx` — a rounded border-bottom/right accent colored by the selected project's `colorHex` (falling back to the default crimson token when unset), paired with a matching `boxShadow` and a `clip-path` restricting the shadow to the bottom/right edges only (top/left stay clean). User confirmed this renders correctly in the running app — ticket marked Done.

---

### DS03.2 — Kanban columns (one per task status) ✅

**File:** `frontend/packages/web/src/features/board/KanbanBoard.tsx`

**Note on scope correction (acceptance criteria updated from the original ticket text):** the ticket as originally written listed columns as `Backlog, Todo, InProgress, InReview, Done`, which does not match the actual `TaskStatus` enum (`frontend/packages/shared/src/ApiClient.ts:111`): `Backlog | InProgress | Blocked | Done | Cancelled`. Consulted the user, who decided: (1) build columns from the real backend enum rather than the ticket's stated list, and (2) `Cancelled` tasks are excluded from the board entirely — no column for them. The acceptance criteria below reflect that decision, not the original ticket text.

**Acceptance criteria:**
- One column per non-`Cancelled` `TaskStatus` value: `Backlog`, `InProgress`, `Blocked`, `Done` — ✅ `Cancelled` tasks filtered out via `tasks.filter(t => t.status !== 'Cancelled')`, no column rendered for that status
- Column header shows status label + task count badge — ✅ `border-b border-border-subtle` header, `text-text` label, `bg-surface-overlay text-text-muted` count badge
- Column body: `bg-surface` with a `border border-border` and `rounded-lg`; scrollable vertically if many tasks — ✅
- Empty column shows a muted placeholder, not blank space — ✅ dashed `border-border-subtle` panel reading "No tasks" in `text-text-muted`
- Columns are horizontally scrollable if viewport is narrow — ✅

Also restyled in the same pass (same file, same ticket scope): drag-over highlight changed from raw blue to `border-primary-700 bg-primary-900/10`, replacing the prior ad-hoc Tailwind grays/blues/reds throughout the component with design-system tokens. The existing native-HTML5-dragdrop transition logic (calls `transitionTask`) was left untouched by the restyle.

Verified in the running app: created temporary test tasks via direct API calls covering all four rendered statuses plus one `Cancelled` task, confirmed the four columns render correctly, the `Cancelled` task does not appear in any column, and drag-drop between columns still triggers `transitionTask` correctly after the restyle. Temporary test data cleaned up from Postgres afterward.

---

### DS03.3 — Task card component in new design system ✅

**File:** `frontend/packages/ui/src/components/TaskCard/TaskCard.tsx` (corrected — ticket originally listed the stale path `frontend/packages/web/src/features/board/TaskCard.tsx`; the component actually lives in `packages/ui`)

**Note on file organization:** Both `PriorityBadge` and `TaskCard` were moved from flat files into the project's folder + `.styles.ts` + `index.ts` convention (matching `ViewToggle`, `ProjectCard`, etc. — see [MISSION.md](MISSION.md#frontend--npm-workspaces-monorepo)):
- `frontend/packages/ui/src/components/PriorityBadge/` — restyled from a filled pill to a colored dot + label.
- `frontend/packages/ui/src/components/TaskCard/` — restyled per the acceptance criteria below.

**Acceptance criteria:**
- `border-border rounded-md` — ✅. Background is `bg-surface-overlay`, not the ticket's originally stated `bg-surface-raised` — **deviation, confirmed with the user:** the Kanban column background was changed to `bg-surface-raised` in DS03.2's floating-column follow-up this same sprint, so cards need to sit one elevation level higher to stay visible against the column; `bg-surface-raised` would have made cards blend into the column.
- Shows priority badge (colored dot + label) + title + due date (red/`primary-700` if overdue, matching the app's existing overdue/error convention) + assignee avatar — ✅
- Priority badge colors, exactly per ticket spec: Critical = `primary-800`, High = `orange-500`, Medium = `yellow-500`, Low = `text-muted` — ✅ mapped to `bg-text-muted` since it's a design token, not a literal Tailwind "zinc" color
- Hover — **deviation, iterated live with the user:** no resting-state shadow (flat until hover); on hover, a crimson-tinted glow (`shadow-[0_8px_28px_-4px_rgba(185,28,28,0.35)]`) plus a 1px lift (`hover:-translate-y-px`), border brightening (`hover:border-border-subtle`), and `active:scale-[0.99]` — deliberately matching `ProjectCard`'s existing hover treatment for consistency, per the user's explicit request. Transition is `duration-500 ease-out` (tuned by the user directly in the file) so the glow visibly grows in on hover rather than snapping.
- Clicking a card opens the DS05 detail panel — ✅, but required a bug fix (see below); note DS05 (the "real" detail panel ticket) doesn't exist yet as a formal ticket, so this reuses the existing ad-hoc panel from `ListView.tsx`, not a new DS05-built one.

**Other deviations from the original ticket text:**
- Dropped the `StatusIndicator` dot that used to render on the card — status is now conveyed by column position in Kanban view, so showing it again on the card was redundant. Also removed a dead, never-used `onStatusChange` prop.

**Bug fix required to satisfy the "clicking opens the detail panel" criterion:** clicking cards in Kanban view was a no-op — `BoardScreen` never passed an `onTaskClick` handler to `KanbanBoard`, and the only existing detail panel was hardcoded inside `ListView.tsx` (self-contained, not reusable). Fixed by extracting that panel into a new shared component, `frontend/packages/web/src/features/board/TaskDetailPanel/` (folder + `.styles.ts`, same convention — styling kept as-is, not retokenized, just reorganized), lifting `selectedTask` state up to `BoardScreen.tsx`, and wiring `onTaskClick={setSelectedTask}` into both `KanbanBoard` and `ListView` so they share one panel instance instead of `ListView` owning its own copy.

Verified via `tsc --noEmit` (clean) at each step. Visual/browser verification was blocked mid-session by the running dev server's module graph getting stuck after the flat-file-to-folder restructuring (a recurring issue this session — the same thing happened during `KanbanBoard`'s move in DS03.2), so in-browser confirmation happened after the user's own restart/testing — the user has confirmed the result looks and behaves correctly.

---

### DS03.4 — Wire drag-drop status transitions ✅

**File:** `frontend/packages/web/src/features/board/KanbanBoard.tsx` (styles: `KanbanBoard.styles.ts`; new `@keyframes toastFade` added to `index.css`)

**Note on scope (carried over from earlier in this epic):** DS03.2 had already shipped a working native HTML5 drag-and-drop implementation that calls `transitionTask()` on drop, moves the card between columns, and highlights the drop-target column. This ticket's actual scope was narrowed to three items:
1. **DnD approach — resolved, no migration performed.** Kept the existing native HTML5 DnD rather than migrating to `@dnd-kit/core`. Touch support and keyboard accessibility aren't needed until the mobile/touch story is in scope. Migration tracked separately as **MOB04** in [ROADMAP.md](ROADMAP.md#post-mvp-backlog)'s post-MVP Mobile App Phase backlog.
2. **BUG-07 fix** — Done/Cancelled tasks can only reopen to Backlog; the board didn't visually reflect this restriction. Fixed — see below.
3. **Toast on failed transition** — the previously-silent revert-on-error now surfaces a toast.

**Acceptance criteria:**
- Cards draggable between columns — ✅ unchanged from DS03.2 (native HTML5 DnD; no `@dnd-kit/core` migration — see MOB04 above)
- Dropping a card calls `transitionTask(taskId, newStatus)` — ✅ unchanged from DS03.2
- Optimistic UI: card moves instantly, reverts on API error — ✅ unchanged from DS03.2, now additionally surfaces a toast on revert (new this ticket, see below)
- Drag indicator: dragged card semi-transparent, drop target column highlights — ✅ unchanged from DS03.2
- Dragging a Done/Cancelled card: only Backlog highlights as a valid drop target, all other columns show a non-droppable state for that drag (fixes BUG-07) — ✅ new this ticket, see below

**BUG-07 fix — implementation:** added `draggedTask` state (the task currently being dragged), a derived `restrictedToBacklog` flag (true when `draggedTask.status` is `Done` or `Cancelled`), and an `isLockedOut(status)` helper (true for every column except `Backlog` while `restrictedToBacklog` is true). `onDragOver` skips `e.preventDefault()` for locked-out columns — since the drop is never "allowed" at the browser level, the browser natively shows a disallowed-drop cursor and no `drop` event ever fires there, so an invalid transition can no longer reach the API at all (rather than reaching it and failing after the fact). Locked-out columns render a new `columnInvalid` style (`opacity-40 cursor-not-allowed`) in place of the normal idle/drag-over states, scoped to only that specific drag. Verified live: dragging a `Done` task onto `InProgress` was rejected client-side (status unchanged in Postgres afterward, confirmed via direct API check); the same task dropped onto `Backlog` succeeded (status changed, confirmed via API).

**Toast-on-failure — implementation:** added directly in `KanbanBoard.tsx`/`KanbanBoard.styles.ts` — no new shared toast component, scoped to this one use case. On a failed `transitionTask()` call (now only reachable for genuine network/server errors, since the BUG-07 fix prevents the invalid-transition case client-side), the card reverts as before and a toast renders: `Error | Couldn't move "<task title>" — try again.` Visual design iterated live with the user to match the app's existing floating-pill chrome (`FloatingPill`/`FloatingNav`): `rounded-full` (relaxing to `rounded-2xl` below the `sm` breakpoint — see responsive note below), `bg-surface-raised/90 backdrop-blur-md`, and a static crimson glow (`shadow-[0_0_14px_3px_rgba(185,28,28,0.3)]` — the same value the other pills use as their `motion-reduce` fallback; needed because a plain `shadow-lg` was nearly invisible against the dark theme's surface tones). Auto-dismiss extended from an initial 3s to 15s via `setTimeout`, paired with a new `toastFade` `@keyframes` (added to `index.css` alongside the existing `pillGlow`/`panelGlow`/`swatchGlow`) that holds full opacity until 95% of the animation then fades over the last ~5% — so the dismiss duration can be tuned later without hand-tuning the fade portion separately. Added a manual close (`X`, `lucide-react`, matching the icon-import convention already used for `ListChecks`/`LayoutGrid`/`List` elsewhere) that dismisses immediately rather than waiting out the timer. Added an "Error" title (bold, `text-primary-700`) before the message, separated by a `toastDivider`, matching `FloatingNav`'s own tab-divider pattern.

**Real bug found and fixed during this ticket — a false-positive verification story worth preserving:** the toast was initially placed at `bottom-6`, which put it directly behind `FloatingNav` (a `fixed bottom-4 z-50` pill that renders later in the DOM, so it painted on top and completely hid the toast at equal z-index). The toast was being added to the DOM correctly and its trigger logic was correct — **DOM presence alone was not sufficient evidence the feature worked**, since nothing was actually visible on screen. Caught via `document.elementFromPoint` at the toast's own center returning `FloatingNav`, not the toast itself. Fixed by moving the toast to `top-8` (clear of both `FloatingNav` at the bottom and `FloatingPill` at `top-4 right-4`); re-verified via `elementFromPoint` that the toast is now the topmost element at its own center. Lesson for future verification: for any fixed/floating-position UI, confirm actual topmost-at-point visibility rather than just DOM presence when other fixed-position siblings exist.

**Responsive layout (below the `sm` / 640px breakpoint):** title and close button reflow onto their own row together (`justify-between` spreads them to opposite edges) while the message wraps to a second row alone — achieved via `max-sm:basis-full` on the message plus `max-sm:order-1`/`max-sm:order-2` reordering (close before message), keeping it a single flex container rather than a hardcoded two-row DOM structure. Divider hidden and corner radius relaxes to `rounded-2xl` at this breakpoint since a two-line pill looks wrong fully rounded. Verified via live geometry checks at both 375px width (two rows confirmed via bounding-rect comparison) and 1280px width (single row, unchanged height).

**Verification method throughout:** `tsc --noEmit` clean at every step. Live browser verification of the failure path used a forced-failure technique (monkey-patching `XMLHttpRequest.prototype.open` to redirect only `/transition` calls to an invalid port) rather than requiring the real backend to be down, since it reliably reproduces a network failure without disrupting the user's own dev environment — confirmed no task-state corruption resulted from any test drag afterward via direct API checks.

---

## DS04 Tickets

**Epic:** Task creation modal (web, inline — not FAB) — ✅ Done (Sprint 1b)

| Ticket | Title | Status |
|---|---|---|
| DS04.1 | Modal shell component | ✅ Done |
| DS04.2 | Task form fields (title, priority, due date) | ✅ Done |
| DS04.3 | Assignee selector | ✅ Done |
| DS04.4 | Wire submit → createTask() + refetch | ✅ Done |

---

### DS04.1 — Modal shell component ✅

**File:** `frontend/packages/web/src/components/Modal.tsx` + `Modal.styles.ts`

**Acceptance criteria:**
- Reusable `<Modal isOpen onClose title children footer />` component — ✅
- Backdrop: `bg-black/60` scrim; clicking outside calls `onClose`; `Escape` key also closes — ✅ both verified
- Modal panel: `bg-surface-overlay border border-border rounded-xl`; centered on screen; `max-w-lg w-full` — ✅
- Subtle entrance animation (fade + slight scale up, CSS only — no animation library) — ✅ `modalIn` keyframe added to `frontend/packages/web/src/index.css`
- Locks body scroll while open — ✅

**Follow-up (user feedback):** the panel shadow was changed from a plain `shadow-2xl` to reuse the same animated crimson `panelGlow` glow already used by `CreateProjectModal` (DS02.3), for visual consistency across every modal in the app — now `animate-[modalIn_180ms_ease-out,panelGlow_3s_ease-in-out_infinite]`.

Verified in-browser: Escape key closes the modal and releases the body-scroll lock; `panelGlow` animation confirmed active on the panel. `npm run build --workspace=packages/web` passed with no type errors.

---

### DS04.2 — Task form fields ✅

**File:** `frontend/packages/web/src/features/tasks/TaskForm.tsx` + `.styles.ts`

**Acceptance criteria:**
- **Title** — autofocused text input, required, max 300 chars — ✅
- **Priority** — four pill buttons: Low / Medium / High / Critical; Medium pre-selected; selected pill uses `bg-primary-800 text-text-inverted` — ✅ verified in-browser that the active pill class is applied correctly (High selected during the manual test flow)
- **Due date** — optional; native `<input type="date">`; "No due date" hint; clearable — ✅
- State is local; form values passed up to parent via `onSubmit(values)` — ✅

---

### DS04.3 — Assignee selector ✅

**File:** `frontend/packages/web/src/features/tasks/TaskForm.tsx` (inline in form, per the ticket's own file path — no separate component was split out)

**Acceptance criteria:**
- Horizontal scroll row of user avatars loaded from `useUsers()` — ✅
- First item: "Unassigned" (zinc circle with `—`) — ✅
- Selected avatar gets a `ring-2 ring-primary-800`; tapping again deselects — ✅
- Hidden if no users exist — ✅

**Bug fix (user feedback):** the avatar row's `overflow-x-auto` was implicitly forcing `overflow-y: auto` (per the CSS spec, one non-`visible` axis forces the other non-`visible` too), which clipped the selected avatar's ring box-shadow at the top and bottom edges (and would have clipped left/right as well). Fixed by adding `px-1 py-1` padding to the row so the ring has room to render fully. Verified in-browser via computed styles/`getBoundingClientRect()` that the ring now fits entirely within the row's clipping box.

---

### DS04.4 — Wire submit ✅

**File:** `frontend/packages/web/src/features/tasks/CreateTaskModal.tsx`

**Acceptance criteria:**
- Composes `Modal` + `TaskForm` — ✅
- On submit: calls `createTask()` from `@taskmanager/shared` — ✅
- Closes modal on success; awaits the parent's refetch callback before closing — ✅
- Submit button disabled while title is empty; shows "Creating…" during the async call — ✅
- API errors rendered inline below the form (not silently swallowed) — ✅

**Wiring:** also wired into `frontend/packages/web/src/screens/BoardScreen/BoardScreen.tsx` (+ `.styles.ts`) — added a "+ New Task" button in the header (next to the view toggle, only shown when a project is selected) which opens `CreateTaskModal`; `useTasks`'s `refetch` is passed through so the board list updates immediately after a task is created.

Verified: `npm run build --workspace=packages/web` passed with no type errors, both before and after the DS04.1/DS04.3 follow-up fixes. Full manual flow tested live in-browser against the running dev server: opened the modal, confirmed submit was disabled with an empty title, filled in a title, selected High priority and an assignee, submitted, confirmed the new task card appeared in the Backlog column immediately with no console errors, and confirmed Escape closes the modal and releases the body-scroll lock.

---

## DS05 Tickets

**Epic:** Task detail panel — ✅ Done (Sprint 1b)

| Ticket | Title | Status |
|---|---|---|
| DS05.1 | Detail panel shell (slide-in drawer) | ✅ Done |
| DS05.2 | Field renderers: status, priority, phase, assignee, due date | ✅ Done |
| DS05.3 | Inline title and description editing | ✅ Done |
| DS05.4 | Wire all mutations + refetch on close | ✅ Done |

---

### DS05.1 — Detail panel shell (slide-in drawer) ✅

**File:** `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx` + `TaskDetailPanel.styles.ts`

**Note on file replacement:** this new component replaces the old bottom-sheet-style panel that lived at `frontend/packages/web/src/features/board/TaskDetailPanel/` (deleted as part of this ticket — confirmed via grep it had exactly one consumer, `BoardScreen.tsx`, which was updated to import the new component instead).

**Acceptance criteria:**
- Slides in from the right; `fixed inset-y-0 right-0 w-[480px]`; `bg-surface-raised border-l border-border` — ✅
- Backdrop scrim on the left; clicking it closes the panel — ✅
- Panel has a header: task title (editable, DS05.3) + close button — ✅
- Smooth CSS slide transition (`transform translateX`) — ✅ new `drawerIn` keyframe added to `frontend/packages/web/src/index.css`
- Scrollable body if content overflows — ✅

Also added, beyond the ticket's explicit text, for consistency with DS04's `Modal.tsx` pattern: Escape-to-close and a body-scroll lock while the panel is open.

Verified: `npm run build --workspace=packages/web` (tsc + vite) passed cleanly. Escape key closes the panel and releases the body-scroll lock (`document.body.style.overflow` back to `''`), confirmed in-browser with no console errors.

---

### DS05.2 — Field renderers ✅

**File:** `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx`

**Acceptance criteria:**
- **Status** — 5-pill selector (all `TaskStatus` values, including Cancelled); changing calls `transitionTask()` — ✅
- **Priority** — 4-pill selector; changing calls `updateTask()` — ✅
- **Phase** — pill selector from `usePhases()`; changing calls `updateTask()` / `updateTask(..., {clearPhase:true})`; active phase highlighted correctly using local state — ✅ **fixes BUG-02**: uses local `phaseId` state seeded from the `task` prop rather than reading `task.phaseId` directly, so the highlight updates immediately on click instead of staying pinned to the stale prop
- **Assignee** — avatar row, same pattern as DS04.3's `TaskForm.tsx`; changing calls `updateTask()` / clears via `clearAssignee` — ✅
- **Due date** — native date input; changing calls `updateTask()`; overdue shown via a red "Overdue" label using `text-primary-700` (matches `TaskCard`'s existing overdue color convention) — ✅

**Deliberate scope narrowing:** due-date *clearing* was not implemented — traced the backend (`UpdateTaskCommand.cs`) and confirmed there's no `clearDueDate` flag (unlike phase/assignee), so a native-clear on the date input just reverts to the last persisted value rather than silently no-op'ing against the API.

Verified in-browser: **reproduced BUG-02's fix directly** by creating two temporary phases via a direct API call, clicking between them, and confirming the active pill highlight updated instantly (screenshotted the computed class list showing only the clicked pill carrying `bg-primary-800`) — temporary phases deleted afterward to leave the seed DB clean. Set a due date and confirmed the "Overdue" red label appeared. No console errors.

**Follow-up — Edit/Save/Cancel rework (user-requested reversal, verified in-browser; see DS05.3/DS05.4 follow-ups below for the full story):** these field renderers (status/priority/phase/assignee/due-date pill selectors and inputs) now only render while the panel is in edit mode, and changing them stages a value into local edit-state instead of calling `transitionTask()`/`updateTask()` immediately — superseding the "changing calls X()" wording in the acceptance criteria above. The BUG-02 fix itself is unaffected: the phase highlight still reads from local state rather than the `task` prop, it's just staged edit-state now instead of immediately-saved state, so the highlight still updates instantly on click.

---

### DS05.3 — Inline title and description editing ✅

**File:** `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx`

**Acceptance criteria (original ticket, as shipped):**
- Title: clicking it turns into an `<input>`; blurring or pressing Enter saves via `updateTask()` — ✅ *(superseded — see follow-up below)*
- Description: clicking it turns into a `<textarea>`; blurring saves via `updateTask()` — ✅ *(superseded — see follow-up below)*
- Both show a subtle edit affordance on hover (pencil icon or underline) — ✅ Lucide `Pencil` icon revealed on hover via `group`/`group-hover` *(superseded — see follow-up below)*
- Unsaved changes not lost if user clicks a field renderer — ✅ relies on the browser's blur-before-click event ordering, which fires the save before the new click handler runs *(no longer applicable — see follow-up below)*

Verified in-browser (original implementation): edited description and title inline and confirmed both persisted via `PUT /api/tasks/{id}` and survived a full page reload. No console errors.

**Follow-up — user-requested reversal (verified in-browser):** after shipping, the user gave direct feedback: "This needs a Edit and save button, should default to read only" — a deliberate reversal of the always-editable, saves-on-blur pattern this ticket originally built. The panel now defaults to a **read-only view**: title renders as a plain heading and description as plain text (or "No description"), with no click-to-edit affordance and no per-field hover pencil icon. An **"Edit" button** in the panel header (see DS05.4's follow-up) switches the panel into edit mode, which reuses this ticket's original inline title `<input>` and description `<textarea>` — but editing them now only updates local React state; blur/Enter no longer trigger `updateTask()`. Saving is deferred to DS05.4's batched Save action.

**Acceptance criteria — reinterpreted for the new behavior:**
- Title/description render as plain read-only text on panel open, not editable controls — ✅
- Clicking "Edit" (header button) switches title/description into the original `<input>`/`<textarea>` controls — ✅ same controls this ticket built, reused rather than rebuilt
- Editing while in edit mode updates local state only; no `updateTask()` call fires until Save (DS05.4) — ✅
- Cancel (DS05.4) discards these staged edits and returns to the read-only view showing the last-saved values — ✅

Verified in-browser as part of the same session that reworked DS05.4: entering edit mode and typing in the title/description fields fired zero network requests; clicking Save persisted both fields in the same batched `updateTask()` call described in DS05.4's follow-up.

---

### DS05.4 — Wire all mutations + refetch on close ✅

**File:** `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx`, `frontend/packages/web/src/screens/BoardScreen/BoardScreen.tsx`

**Acceptance criteria (original ticket, as shipped):**
- All mutations (`transitionTask`, `updateTask`) called immediately on change (no Save button) — ✅ *(superseded — see follow-up below)*
- Panel accepts a `refetch` prop from `BoardScreen`; calls it when the panel closes — ✅ **fixes BUG-03**: called inside the panel's own `handleClose`, used by the backdrop click, the header close button, and Escape
- API errors surfaced inline per field — not silently swallowed — ✅ caught per-field into an `errors` record and rendered inline under the relevant section *(superseded — see follow-up below)*
- Board cards reflect mutations after panel closes — ✅

**Wiring:** `BoardScreen.tsx` updated — import path switched from the deleted `features/board/TaskDetailPanel/` to the new `features/tasks/TaskDetailPanel.tsx`, and `refetch={refetchTasks}` (from the existing `useTasks` hook) threaded through as the panel's `refetch` prop.

Verified (original implementation): `npm run build --workspace=packages/web` (tsc + vite) passed cleanly. Full manual flow tested live against the running dev server: opened the panel from both Kanban and List views, confirming it's the same shared component instance in both surfaces (satisfies DS06.2's future consolidation goal even though DS06 itself hasn't started); **reproduced BUG-03's fix** by editing a task's title/due-date, closing the panel, and confirming the Kanban card showed the updated title/date immediately without a manual page reload (confirmed via a fresh `GET /api/tasks/project/...` network request firing on close). Also confirmed Escape closes the panel and releases the body-scroll lock. No console errors in any of the above (one stale/unrelated Vite HMR warning appeared right after the old component's files were deleted mid-session, from the dev server's already-loaded module graph — cleared itself on the next full navigation and is not a real app issue). **BUG-03's fix (the `refetch`-on-close wiring) is unchanged by the rework below and remains accurate as originally shipped.**

**Follow-up — Edit/Save/Cancel rework (user-requested reversal, verified in-browser):** the user directly reversed this ticket's "no Save button" acceptance criterion: "This needs a Edit and save button, should default to read only." Implemented and verified:
- An **"Edit" button** in the panel header (next to the existing Close button) switches the panel from its default read-only view (see DS05.3's follow-up) into edit mode, rendering the interactive controls DS05.2/DS05.3 originally built — but editing any of them now only updates local React state; no API call fires per field.
- A footer with **Cancel** and **Save** buttons appears only while in edit mode. Save is disabled while title is empty or a save is already in flight (shows "Saving…").
- **Save** batches everything into a single `transitionTask()` call (only if status actually changed) plus one `updateTask()` call carrying title/description/priority/phase/assignee/due-date together, then returns the panel to read-only view showing the newly saved values. A single `saveError` string (not per-field errors) is shown inline in the footer on failure, and the panel stays in edit mode so the user can retry — this supersedes the original "API errors surfaced inline per field" criterion above.
- **Cancel** discards all staged local edits and returns to read-only view without calling any API.
- Due-date clearing remains unsupported (re-confirmed no `clearDueDate` flag exists on the backend's `UpdateTaskCommand`), so clearing the date input during editing just reverts it locally rather than staging a broken clear.
- **Real bug found and fixed during this rework:** Cancel (and Save's "did status change?" check) initially compared staged edits against the raw `task` prop, which never updates after an in-panel Save (`BoardScreen` only refetches when the whole panel closes, per BUG-03's fix above — not after every in-panel Save). This meant Save → re-enter Edit → Cancel could silently revert the panel's own display back to pre-save values even though the server already had the new ones. Fixed by tracking a local "last-saved snapshot" (seeded from the `task` prop, re-synced after every successful Save) that Cancel and Save's change-detection now compare against instead of the prop directly.

Verified live in-browser: read-only view shows correct persisted values on open; entering edit mode and changing fields fires zero network requests until Save is clicked; Save fires exactly the expected `PUT`/`POST .../transition` calls, both returning 200; the panel returns to read-only showing the new values; re-entering edit mode and clicking Cancel correctly reverts to the last-*saved* values (not the original stale prop) with zero network calls — this specifically re-tested the last-saved-snapshot bug above after the fix. `tsc --noEmit` + `vite build` passed clean throughout.

---

## DS06 Tickets — Restyle List View

**Epic:** Restyle List view in the new design system — ✅ Done (Sprint 1b)

> `ListView.tsx` never got a DS0x-series pass at the time this epic was written — it was still the original Sprint 1 implementation. DS06.1 restyled the rows/chrome (and moved the file into its own directory, per the DS02.1 convention). DS06.2's original premise — that `ListView` had its own embedded `TaskDetailPanel`, separate from DS05's standalone one, needing consolidation — turned out to be stale by the time it was reached: DS05 shipped after this epic's tickets were originally written, and it already gave `BoardScreen` a single shared `TaskDetailPanel` instance used by both Kanban and List view. There was never a second panel to consolidate away; DS06.2 closed as a no-op-but-verified ticket confirming that architecture live.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| DS06.1 | Restyle list rows and chrome | ✅ Done | DS01 |
| DS06.2 | Consolidate onto DS05's TaskDetailPanel | ✅ Done | — |

---

### DS06.1 — Restyle list rows and chrome ✅

**File:** `frontend/packages/web/src/features/board/ListView/` — moved into its own directory (`ListView.tsx`, `ListView.styles.ts`, `index.ts`) per the DS02.1 file-organization convention; was previously a flat file left over from Sprint 1, the only remaining component that hadn't been migrated to that convention.

**Acceptance criteria:**
- List rows use `bg-surface-raised border border-border` (or equivalent) design tokens — no raw Tailwind color classes remain anywhere in the file — ✅ every raw class (`bg-white`, `text-gray-800`, `border-gray-100`, `text-red-500`, and their `dark:` pairs) replaced with tokens (`bg-surface-raised`, `border-border`, `text-text`, `text-text-muted`, `text-primary-700`)
- Row hover/selected states styled consistently with `TaskCard`'s hover treatment (DS03.3) — ✅ hover → `bg-surface-overlay` with the same crimson shadow formula (`rgba(185,28,28,0.3)`) TaskCard uses; press feedback uses `active:scale-[0.995]` rather than TaskCard's own `0.99` — a deliberate softer adaptation since a full-width row reads differently under scale than a card
- Empty state (no tasks) styled consistently with the empty states already shipped in `ProjectSelectorScreen`/`BoardScreen` (icon + copy, not a bare text dump) — ✅ icon + "No tasks yet" / "Create your first task to get started" copy, matching the established pattern. Note: not reachable through normal navigation today since `BoardScreen` already gates on `tasks.length > 0` before mounting `ListView` — included as a defensive/reusability safeguard since the acceptance criterion explicitly required it
- Both dark and light modes verified — ✅ with one caveat, see below

Verified via `npm run build --workspace=packages/web` (tsc + vite, full from-scratch compile), passing clean both immediately after the restyle and again after the directory move. Manually verified in-browser: all row fields render correctly (status dot, title, priority badge, phase tag, due date, assignee avatar); clicking a row still correctly opens the existing `TaskDetailPanel` (DS05) with no regression; compiled CSS output inspected directly and confirmed `bg-surface-raised` resolves to `background-color:var(--color-surface-raised)`, the same token-resolution pattern already proven working elsewhere. **Caveat:** a live dark/light-mode visual screenshot comparison wasn't obtainable this session — the browser pane wasn't compositing frames and `getComputedStyle` reads were inconsistently stale even against unrelated pre-existing shipped code, pointing to a session/environment rendering artifact rather than an app bug. Confidence rests on the compiled-CSS-level check plus the token pattern already verified correct elsewhere (per DS03.1's live computed-style verification of the same token system on a different component).

---

### DS06.2 — Consolidate onto DS05's TaskDetailPanel ✅

**File:** `frontend/packages/web/src/features/board/ListView/`

**Goal (as originally written):** Stop maintaining two divergent detail panel implementations — `ListView`'s own embedded panel and DS05's standalone `features/tasks/TaskDetailPanel.tsx`.

**Closed as a no-op — the premise didn't match the code by the time this ticket was reached.** DS05 (built after DS06's tickets were originally written) already shipped with `BoardScreen` rendering exactly one shared `<TaskDetailPanel task={selectedTask} users={users} phases={phases} onClose={...} refetch={refetchTasks} />` instance, conditional on `selectedTask` state, completely independent of `viewMode` — both `KanbanBoard` and `ListView` feed into the same `setSelectedTask` callback via their respective `onTaskClick` props. `ListView.tsx` itself has never contained any panel-rendering code, in either its pre-DS06.1 or current (post-restyle) form — it only accepts an `onTaskClick?: (task: ProjectTaskDto) => void` prop and calls it from each row's `onClick`. There was no second, List-specific panel implementation to remove or consolidate.

**Acceptance criteria — rewritten as confirmed-already-true given the architecture DS05 shipped with, rather than describing work that was performed:**
- Clicking a row in List view opens DS05's standalone `TaskDetailPanel` — confirmed true; it always has, since `BoardScreen` wires `ListView`'s `onTaskClick` to the same `setSelectedTask` used by Kanban
- Opening a task from List view and from Kanban shows the same component instance, not a visual clone — confirmed live in-browser (identical Edit/Save UI and behavior on both surfaces)
- BUG-02 and BUG-03 are resolved for the List view surface — confirmed true, for a stronger reason than "consolidation": there was never a List-specific panel implementation that could diverge from the Kanban one, so both fixes are inherited structurally rather than by a migration step
- No behavior regression: all fields editable from the shared panel (status, priority, phase, assignee, due date, title, description) remain editable in List view context — confirmed via a live edit: changed a task's priority Critical→High through Edit→Save from the List surface, confirmed via network inspection that `PUT /api/tasks/{id}` carried `"priority":"High"` and returned 200
- Verified in-browser (BUG-03 check, List surface specifically): closed the panel after the above edit and confirmed the List view row itself updated immediately to show "High" with no manual page reload
- BUG-02's fix (local `phaseId` state instead of the stale prop) verified as structurally inherited for free — same component instance/class, nothing List-specific to test differently

No code was changed to close this ticket. Re-verification was performed directly against the current, post-DS06.1 code to rule out regression, not to confirm new work.

---

## A01 Tickets — Admin Panel Shell & Guardrails

**Epic:** Build desktop admin panel shell — ✅ Done (Sprint 2)

> Verified directly against source on 2026-08-23 — implemented but never marked done in these docs until then. `AdminGuard.tsx`, `AdminShell.tsx`, and an `isAdmin`-gated `FloatingPill` entry point all confirmed present and matching spec.

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| A01.1 | Desktop-only guardrail (media-query lock) | 3 | ✅ Done | — |
| A01.2 | Admin shell layout (side nav + content area) | 5 | ✅ Done | — |
| A01.3 | Admin entry point in main app nav | 2 | ✅ Done | A01.1, A01.2 |

---

### A01.1 — Desktop-only guardrail (media-query lock) ✅

**File:** `frontend/packages/web/src/screens/admin/AdminGuard.tsx` (new)

**Points:** 3

**Goal:** Block admin routes entirely on non-desktop viewports — no partial or broken admin UI ever renders on mobile.

**Acceptance criteria:**
- Guard checks viewport width (e.g. `>= 1024px`) via `window.innerWidth` or `matchMedia`
- Below the threshold, renders a clear "Admin requires a desktop screen" message instead of any admin content
- Re-evaluates on window resize, not just initial mount — resizing below the threshold while already inside the admin panel locks it out immediately
- Verified in-browser: resizing above/below the threshold toggles live between the locked-out message and admin content, no console errors

---

### A01.2 — Admin shell layout (side nav + content area) ✅

**File:** `frontend/packages/web/src/screens/admin/AdminShell/` (new)

**Points:** 5

**Goal:** The persistent chrome every admin screen renders inside.

**Acceptance criteria:**
- Side navigation with links: User Management (routes to A02.3's screen), Webhook Manager (visually present but disabled/greyed — A03 is deferred post-MVP and not yet built)
- Active nav link highlighted using design tokens, consistent with `FloatingNav`'s active-tab convention
- Content area renders whichever admin screen is currently routed/selected
- Always wrapped by A01.1's guard — `AdminShell` never renders without the guard passing
- Uses design tokens throughout — no raw Tailwind color classes
- Verified in-browser: navigating between nav links swaps the content area correctly; the disabled Webhook Manager link is visibly non-interactive

---

### A01.3 — Admin entry point in main app nav ✅

**File:** `frontend/packages/web/src/App/FloatingNav/FloatingNav.tsx` or `FloatingPill` (extended)

**Points:** 2

**Goal:** Give admins a way to actually reach the admin panel from the main app — nothing links to it today.

**Acceptance criteria:**
- Entry point only rendered when the current user's `isAdmin` is `true` (from `AuthContext`) — fully absent from the DOM for non-admins, not just disabled
- Clicking it navigates into `AdminShell`, behind A01.1's guard
- Verified in-browser: an admin user sees and can use the entry point; logging in as a non-admin user confirms it's completely absent

---

## A02 Tickets — User Management Portal

**Epic:** Build user management portal — ✅ Done (Sprint 2)

> `UsersController` previously only had `GetUsers`/`GetById`/`CreateUser` — no endpoint called `AppUser`'s existing `Deactivate()`/`Reactivate()`/`PromoteToAdmin()`/`RevokeAdmin()`/`UpdateAvatarUrl()` domain methods, even though the domain layer already supported all of it. A02.1/A02.2 closed that gap before the UI (A02.3–A02.6) wired into it.

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| A02.1 | Backend: status & role management endpoints | 3 | ✅ Done | — |
| A02.2 | Backend: avatar upload endpoint | 3 | ✅ Done | — |
| A02.3 | Frontend: user list/grid screen | 3 | ✅ Done | A01.2 |
| A02.4 | Frontend: create-user modal | 3 | ✅ Done | A02.3 |
| A02.5 | Frontend: row actions — toggle active, promote/revoke admin | 3 | ✅ Done | A02.1, A02.3 |
| A02.6 | Frontend: avatar upload UI | 2 | ✅ Done | A02.2, A02.3 |

---

### A02.1 — Backend: status & role management endpoints ✅

**File:** `backend/TaskManager.Application/Users/Commands/SetUserActiveCommand.cs` (new), `SetUserAdminCommand.cs` (new), `backend/TaskManager.API/Controllers/UsersController.cs` (extended)

**Points:** 3

**Goal:** Expose `AppUser`'s existing `Deactivate`/`Reactivate`/`PromoteToAdmin`/`RevokeAdmin` domain methods via the API.

**Acceptance criteria:**
- `PUT /api/users/{id}/active` accepts `{ isActive: boolean }`, calls `Deactivate()`/`Reactivate()` accordingly — `[Authorize(Roles = "Admin")]`
- `PUT /api/users/{id}/admin` accepts `{ isAdmin: boolean }`, calls `PromoteToAdmin()`/`RevokeAdmin()` accordingly — `[Authorize(Roles = "Admin")]`
- Revoking admin status is rejected if it would leave zero remaining admin users in the system
- Both endpoints return the updated `AppUserDto`
- Verified: non-admin JWT → `403` on both; admin JWT → `200` with updated fields; attempting to revoke the last remaining admin is rejected with a clear error and the admin count never reaches zero

New `SetUserActiveCommand`/`SetUserActiveCommandHandler` and `SetUserAdminCommand`/`SetUserAdminCommandHandler` in `Application/Users/Commands/`, calling `AppUser`'s existing domain methods directly — no new domain logic needed. `IUserRepository` gained `CountAdminsAsync()` alongside the existing `ExistsAdminAsync()`, backing the revoke-last-admin guard.

**Follow-up bug found during user testing, not in the original ticket scope:** promoting a *second* user to admin threw a raw `DbUpdateException` (`500`) — AUTH02.3's `IX_Users_SingleAdmin` Postgres partial unique index enforces **at most one admin ever**, and the promote endpoint had no awareness of that constraint. The user was asked to clarify intent and explicitly chose **"keep exactly one admin, always"** over allowing multiple admins. `SetUserAdminCommandHandler` was updated to pre-check `ExistsAdminAsync()` before promoting and throw a clean `InvalidOperationException` (→ `422`, via the same global exception-middleware mapping already used for "can't revoke the last admin") instead of letting the DB constraint violation bubble up as a `500`.

**Known gap flagged to the user, not closed by this ticket:** with two independent toggle endpoints, promote is now always blocked once an admin exists, and revoke is always blocked when it's the last (only) admin — so there is currently **no way to ever change who the single admin is**, through either the UI or the API directly. An atomic "transfer admin" operation would be needed to actually support that, and has not been built. Logged as a post-MVP backlog item — see [TASKS.md](TASKS.md#-post-mvp-backlog).

Verified: non-admin JWT → `403` on both endpoints; admin JWT → `200` with updated `AppUserDto` fields on both; promoting a second user while one already exists → `422` (not `500`); revoking the sole remaining admin → `422`, admin count never reaches zero.

---

### A02.2 — Backend: avatar upload endpoint ✅

**File:** `backend/TaskManager.Application/Common/Interfaces/IAvatarStorage.cs` (new), `backend/TaskManager.Infrastructure/Services/LocalAvatarStorage.cs` (new), `backend/TaskManager.Application/Users/Commands/UploadUserAvatarCommand.cs` (new), `backend/TaskManager.API/Controllers/UsersController.cs` (extended), `backend/TaskManager.API/Program.cs` (extended)

**Points:** 3

**Goal:** Let an admin set a user's avatar image, calling the existing `UpdateAvatarUrl` domain method.

**Acceptance criteria:**
- `POST /api/users/{id}/avatar` accepts `multipart/form-data` — `[Authorize(Roles = "Admin")]`
- Image stored locally, consistent with `AvatarUrl`'s existing doc convention of local hosting for data sovereignty
- Rejects non-image content types and oversized files (2MB limit) with `400`
- On success, calls `AppUser.UpdateAvatarUrl()` and returns the updated `AppUserDto` with the new `AvatarUrl`
- Verified: valid image upload → `200`, `AvatarUrl` populated and resolvable; oversized/wrong-type file → `400`; non-admin JWT → `403`

New `IAvatarStorage` interface (`Application/Common/Interfaces/`), mirroring the existing `IPasswordHasher` abstraction pattern, with a `LocalAvatarStorage` implementation (Infrastructure) saving to `wwwroot/avatars/{id}.{ext}` and cleaning up any stale file left under a previous extension on re-upload (so re-uploading a `.png` over an existing `.jpg` doesn't leave both files orphaned on disk). New `UploadUserAvatarCommand`/handler calls `AppUser.UpdateAvatarUrl()`. `UsersController` gained `POST /api/users/{id}/avatar` (`[Authorize(Roles = "Admin")]`, multipart/form-data), rejecting non-image content types and files over 2MB with `400`. `Program.cs` registered `IAvatarStorage` in DI and added `app.UseStaticFiles()` so uploaded avatars resolve at `/avatars/{id}.{ext}`.

**Deliberate scope decision, flagged at the time:** did not add an image-processing library to transcode every upload to a single `.webp` format — no such dependency existed anywhere in the project, and introducing one purely for this endpoint was judged out of scope. Kept the user's original upload extension (png/jpg/webp/gif) instead.

Verified: valid image upload → `200`, `AvatarUrl` populated and resolvable; oversized/wrong-type file → `400`; non-admin JWT → `403`; re-uploading a different-extension image cleans up the previous file rather than leaving an orphan on disk.

---

### A02.3 — Frontend: user list/grid screen ✅

**File:** `frontend/packages/web/src/screens/admin/UserManagementScreen/` (new)

**Points:** 3

**Goal:** The primary admin view of all users.

**Acceptance criteria:**
- Table/grid of all `AppUser`s via `useUsers()`: username, display name, email, active status, admin badge
- Inactive users are visually distinguished (e.g. dimmed row) from active ones
- Uses design tokens throughout — no raw Tailwind colors
- Reachable only through `AdminShell` (A01.2), not a standalone route
- Verified in-browser: table renders real seeded users with correct active/admin indicators

Table via `useUsers()` showing username, display name, email, status badge, and admin badge, styled entirely on design tokens. Inactive rows are visually dimmed. Wired into `AdminShell` (A01.2) in place of its earlier placeholder.

Verified in-browser: table renders real seeded users with correct active/admin indicators; inactive rows are visually distinct from active ones; only reachable through `AdminShell`.

---

### A02.4 — Frontend: create-user modal ✅

**File:** `frontend/packages/web/src/screens/admin/UserManagementScreen/CreateUserModal/` (new)

**Points:** 3

**Goal:** Surface the already-existing `POST /api/users` endpoint from the admin UI — no new backend work needed here.

**Acceptance criteria:**
- "New User" button on `UserManagementScreen` opens a modal with username, display name, email, password, and an `isAdmin` toggle
- Submits via the existing `createUser()` function in `@taskmanager/shared`
- On success, closes the modal and the user list refetches to show the new user immediately
- API errors (e.g. duplicate username/email) rendered inline, not silently swallowed
- Verified in-browser: creating a user succeeds and appears in the list immediately; a duplicate-username attempt shows an inline error

Reuses the shared `Modal` shell (the same pattern DS04's `CreateTaskModal` established), calling the existing `createUser()` function. Added an `isAdmin` toggle to the frontend's `CreateUserRequest` type in `@taskmanager/shared` — the backend's `CreateUserCommand` already supported an admin flag, it just wasn't exposed in the shared API client until now.

Verified in-browser: creating a user succeeds and appears in the list immediately; a duplicate-username attempt shows an inline error rather than failing silently.

---

### A02.5 — Frontend: row actions — toggle active, promote/revoke admin ✅

**File:** `frontend/packages/web/src/screens/admin/UserManagementScreen/` (extended), `frontend/packages/shared/src/ApiClient.ts` (extended)

**Points:** 3

**Goal:** Wire A02.1's endpoints into the user list.

**Acceptance criteria:**
- Each row has a toggle-active action and a promote/revoke-admin action, calling A02.1's endpoints
- Both actions update the row immediately on success — no full page reload
- Attempting to revoke the last remaining admin surfaces A02.1's rejection as an inline error rather than failing silently
- Verified in-browser: toggling active/inactive and promoting/revoking admin both update the UI immediately; attempting to revoke the sole admin shows the expected error

Built directly into `UserManagementScreen`'s row component: Deactivate/Activate and Promote/Revoke-admin buttons call A02.1's endpoints and refetch on success. Added a new `getErrorMessage()` helper to `@taskmanager/shared`'s `ApiClient.ts` that reads the server's actual `{ error }`/`{ errors }` response body instead of axios's generic "Request failed with status code N" — used here so A02.1's "revoke last admin" and "admin already exists" `422` rejections show their real message inline instead of a generic failure string.

Verified in-browser: toggling active/inactive and promoting/revoking admin both update the UI immediately with no full page reload; attempting to revoke the sole admin shows the real backend error message inline (not a generic one); attempting to promote a second admin (see A02.1's follow-up gap) likewise surfaces the real `422` message via `getErrorMessage()`.

---

### A02.6 — Frontend: avatar upload UI ✅

**File:** `frontend/packages/web/src/screens/admin/UserManagementScreen/` (extended), `frontend/packages/shared/src/ApiClient.ts` (extended), `frontend/packages/web/src/features/board/KanbanBoard.tsx`, `frontend/packages/web/src/features/board/ListView.tsx`, `frontend/packages/web/src/features/tasks/TaskForm.tsx`, `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx` (all touched by the follow-up fix below)

**Points:** 2

**Goal:** Wire A02.2's endpoint into the user list.

**Acceptance criteria:**
- Per-row (or per-user-detail) file picker triggers A02.2's upload endpoint
- On success, the row's avatar updates immediately to the new image
- Invalid file type/size errors from A02.2 are surfaced inline, not silently swallowed
- Verified in-browser: uploading a valid image updates the avatar shown in the list; an oversized file shows an inline error

A camera-icon button overlaid on each row's avatar (in `UserManagementScreen`) opens a native file picker and calls A02.2's upload endpoint, with inline error handling via A02.5's `getErrorMessage()` helper.

**Follow-up bug found during user testing, not introduced by this ticket:** avatars weren't rendering *anywhere* in the app — not just the new admin screen. `AppUserDto.avatarUrl` comes back from the API as a relative path (e.g. `/avatars/{id}.jpg`), but the Vite dev server and the .NET API run on different origins, so a bare `<img src="/avatars/...">` resolved against the wrong origin. This was a **latent bug in pre-existing avatar-rendering code**, present since `AssigneeAvatar` was first built — it only surfaced now because A02.6's upload feature was the first time an avatar URL actually got populated from a real upload. Fixed by adding a null-safe `resolveAssetUrl()` helper to `@taskmanager/shared`'s `ApiClient.ts` and applying it at every place that renders an `AssigneeAvatar` with a user's `avatarUrl`: `KanbanBoard.tsx`, `ListView.tsx`, `TaskForm.tsx`, `TaskDetailPanel.tsx` (two call sites), and `UserManagementScreen.tsx`.

Verified in-browser: uploading a valid image updates the avatar shown in the admin list immediately; an oversized file shows an inline error via `getErrorMessage()`; after the `resolveAssetUrl()` fix, uploaded avatars render correctly across the Kanban board, list view, task form, task detail panel, and the admin user list — not just the row that performed the upload.

---

## AUTH01 Tickets — Backend: Authentication & API Tokens

**Epic:** Backend: Authentication & API tokens — ✅ Done (Sprint 4, MVP-Required)

> Prerequisite for PM01 (needs current-user-id) and PRI01 (votes attributed to the current user). See [ROADMAP.md](ROADMAP.md#phase-6-identity--access-mvp-required) for why this was MVP-required — the app had zero authentication anywhere before this epic.

| Ticket | Title | Status |
|---|---|---|
| AUTH01.1 | JWT login endpoint | ✅ Done |
| AUTH01.2 | JWT Bearer middleware + global `[Authorize]` | ✅ Done |
| AUTH01.3 | `ApiToken` entity + admin CRUD | ✅ Done |
| AUTH01.4 | `ApiToken` auth scheme (non-human access) | ✅ Done |
| AUTH01.5 | Restrict `POST /api/users` to admins only | ✅ Done |

---

### AUTH01.1 — JWT login endpoint ✅

**File:** `backend/TaskManager.API/Controllers/AuthController.cs` (new), `backend/TaskManager.Application/Auth/Commands/LoginCommand.cs` (new)

**Goal:** Let a user exchange a username/password for a JWT.

**Acceptance criteria:**
- `POST /api/auth/login` accepts `{ username, password }` and returns `401` on any invalid credential (username not found or password mismatch) — no distinction in the error message between "unknown user" and "wrong password"
- Password verified via the existing `IPasswordHasher`/BCrypt implementation — no new hashing scheme introduced
- On success, returns a signed JWT containing at minimum the user's `id`, `username`, and `isAdmin` claims
- Token has a defined, documented expiry (e.g. 24h) — re-login required after expiry, no silent refresh-token rotation for MVP
- Verified against a real seeded user: correct credentials → 200 + token; wrong password → 401; nonexistent username → 401

`AuthController.Login` → `LoginCommand`/`LoginCommandHandler` (Application) → new `IJwtTokenGenerator`/`JwtTokenGenerator` (Infrastructure, `System.IdentityModel.Tokens.Jwt`, HMAC-SHA256). Signing config lives in a new `Jwt` appsettings section (`Secret`/`Issuer`/`Audience`/`ExpiryHours`, default 24h) bound via `IOptions<JwtSettings>`, mirroring the existing `DatabaseSettings` pattern in `Program.cs`. Token claims: `sub` (user id), `username`, `isAdmin` (string bool, for easy frontend decoding), and a `role` claim (`Admin`/`User`) so AUTH01.2/AUTH01.3 can use ASP.NET's built-in `[Authorize(Roles=...)]` later. `LoginCommandHandler` wraps `IPasswordHasher.Verify` in a try/catch — the seeded `system` placeholder account's `PasswordHash` ("SYSTEM_ACCOUNT_NO_LOGIN") isn't a real bcrypt hash and would otherwise throw and surface as a 500; now treated as a failed check → 401, preserving the "no distinction between unknown user and wrong password" rule.

Verified live via `curl` against a real user created through the existing `POST /api/users` endpoint: correct credentials → 200 + token; wrong password → 401; nonexistent username → 401; `system` account → 401 (not 500).

---

### AUTH01.2 — JWT Bearer middleware + global `[Authorize]` ✅

**File:** `backend/TaskManager.API/Program.cs`

**Goal:** Every endpoint except login requires a valid JWT (or, after AUTH01.4, a valid `ApiToken`).

**Acceptance criteria:**
- `AddAuthentication().AddJwtBearer(...)` configured with the same signing key/issuer used to mint tokens in AUTH01.1
- A global fallback authorization policy requires an authenticated principal for every endpoint by default (`[AllowAnonymous]` explicitly applied only to `POST /api/auth/login`)
- Existing `ProjectsController`, `TasksController`, `PhasesController`, `UsersController` all return `401` when called without a token, and succeed unchanged when called with a valid one
- Verified: a call to any existing GET endpoint without an `Authorization` header returns `401`; the same call with a valid token from AUTH01.1 returns the expected `200` payload unchanged

`Program.cs` calls `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)`, reading the same `Jwt` config section (`Secret`/`Issuer`/`Audience`) AUTH01.1's `JwtTokenGenerator` signs with, so tokens minted at login validate cleanly. `AddAuthorization` sets a `FallbackPolicy` of `RequireAuthenticatedUser()` — every endpoint is locked down unless explicitly opted out, rather than requiring `[Authorize]` sprinkled per-controller. `AuthController.Login` got `[AllowAnonymous]` as the one opt-out. `app.UseAuthentication()` added ahead of the existing `app.UseAuthorization()`. (Superseded by AUTH01.4's rework below — the single `AddJwtBearer` default scheme was later replaced by a policy scheme that also forwards to an `ApiToken` scheme.)

Verified live via `curl` against all four existing controllers: `GET /api/users`, `/api/projects`, `/api/tasks/{id}`, `/api/phases` all → 401 with no `Authorization` header and → 401 with a garbage token; `/api/users` → 200 with an unchanged payload using a real token from `/api/auth/login`; login itself still succeeds with no token at all.

---

### AUTH01.3 — ApiToken entity + admin CRUD ✅

**File:** `backend/TaskManager.Domain/Entities/ApiToken.cs` (new), EF migration, `backend/TaskManager.Application/ApiTokens/` CQRS folder (new), `backend/TaskManager.API/Controllers/ApiTokensController.cs` (new)

**Goal:** Give non-human/service accounts (Home Assistant, external automation scripts) a way to authenticate that isn't a human login.

**Acceptance criteria:**
- `ApiToken` entity: `Id`, `Name` (human-readable label), `TokenHash` (never store the raw token), `CreatedAt`, `CreatedByUserId`, `RevokedAt` (nullable)
- `POST /api/api-tokens` (admin-only, `isAdmin` claim required — returns `403` otherwise) generates a new token, returns the **raw token value exactly once** in the response body; only the hash is persisted
- `GET /api/api-tokens` lists tokens (name, created date, revoked status — never the raw value again)
- `DELETE /api/api-tokens/{id}` (or a revoke endpoint) sets `RevokedAt`, immediately invalidating that token for auth purposes
- Verified: non-admin user gets `403` on all three endpoints; admin can generate, list, and revoke; a revoked token fails AUTH01.4's auth check

`ApiToken` domain entity (flat properties, not `AuditInfo` — no `UpdatedAt` concept, just `CreatedAt`/`RevokedAt`) with an idempotent `Revoke()` method. New `IApiTokenRepository`/`ApiTokenRepository` (generic, no extra methods yet — AUTH01.4 added what it needed) wired into `IUnitOfWork.ApiTokens`. EF migration `AddApiToken` adds the `ApiTokens` table with a `Restrict`-delete FK to `Users.CreatedByUserId` (no navigation property either side — informational only, via the `HasOne<AppUser>()` no-navigation overload). New `TaskManager.Application/ApiTokens/` CQRS folder: `CreateApiTokenCommand` (generates a 48-char hex raw token via `RandomNumberGenerator.GetHexString`, hashes it by reusing the existing `IPasswordHasher` — same bcrypt hasher AUTH01.1 uses for passwords, since its `Hash`/`Verify` contract is generic, not password-specific), `RevokeApiTokenCommand`, `GetApiTokensQuery` (ordered newest-first). `ApiTokensController` at `[Route("api/api-tokens")]` (explicit kebab-case route, not the `[controller]` token convention) carries a controller-level `[Authorize(Roles = "Admin")]` — this works because AUTH01.1's `JwtTokenGenerator` already stamps a `ClaimTypes.Role` claim (`Admin`/`User`) derived from `isAdmin`, so ASP.NET's built-in role-based authorization "just works" without a custom policy. `CreatedByUserId` is pulled from the JWT's `sub` claim in the controller (no `ICurrentUserService` abstraction exists yet — deferred to PM01.2 which will need the same thing for project membership).

Verified live via `curl` with a real non-admin and a real admin JWT: non-admin → `403` on GET/POST/DELETE; admin → `200` list (empty, then containing the created token), `201` + raw token on create (confirmed the raw value never reappears in the subsequent list call), `204` + `revokedAt` populated on delete, `404` deleting an unknown id.

**Discovered along the way:** `POST /api/users` had no role restriction post-AUTH01.2 — any authenticated user could create new users, including setting `isAdmin: true` on them. Out of scope for this ticket, flagged as a follow-up and resolved in AUTH01.5.

---

### AUTH01.4 — ApiToken auth scheme ✅

**File:** `backend/TaskManager.API/Program.cs`, `backend/TaskManager.API/Authentication/` (new)

**Goal:** Let a request authenticate with a valid `ApiToken` instead of a JWT.

**Acceptance criteria:**
- A request carrying a valid, non-revoked `ApiToken` in a header (e.g. `X-Api-Token`) is authenticated without a JWT
- An invalid or revoked token in that header returns `401`
- Both auth schemes (JWT Bearer and `ApiToken`) work side-by-side on the same endpoints — a human with a JWT and a service account with an `ApiToken` can both call the same protected routes
- Verified: valid `ApiToken` header succeeds against a protected endpoint; revoked token fails; missing both JWT and `ApiToken` fails with `401`

Rather than listing both schemes on every `[Authorize]`/`FallbackPolicy`, `Program.cs`'s `AddAuthentication` registers a `"SmartAuth"` policy scheme (`AddPolicyScheme`) as the default/challenge scheme, with a `ForwardDefaultSelector` that routes to a new `"ApiToken"` scheme when the request carries an `X-Api-Token` header, else to the existing JWT Bearer scheme — so every consumer (the AUTH01.2 fallback policy, `ApiTokensController`'s `[Authorize(Roles = "Admin")]`, any future controller) accepts either credential type with zero per-endpoint changes. New `ApiTokenAuthenticationHandler`/`ApiTokenAuthenticationDefaults` (`backend/TaskManager.API/Authentication/`) implement the `ApiToken` scheme: pulls the raw value from `X-Api-Token`, scans non-revoked `ApiToken` rows and runs each through the existing `IPasswordHasher.Verify` (bcrypt hashes are salted, so a token can't be looked up by equality — documented as an accepted O(n) tradeoff given expected token counts are small) until one matches, then builds a `ClaimsPrincipal` carrying `CreatedByUserId` as `NameIdentifier` plus `apiTokenId`/`apiTokenName` — deliberately no `Admin` role claim, so a service-account token can hit ordinary protected endpoints but still gets `403` from `ApiTokensController`, same as any other non-admin principal.

Verified live: JWT auth unaffected (regression-checked against `/api/projects`); a valid `ApiToken` header succeeds on `/api/projects` and `/api/users`; a bogus token header and no auth at all both → `401`; revoking the token via AUTH01.3's endpoint immediately turns its previously-valid header into a `401`; the same valid `ApiToken` gets `403` (not `200`) against the admin-only `ApiTokensController`, confirming the two schemes don't blur privilege boundaries.

---

### AUTH01.5 — Restrict `POST /api/users` to admins only ✅

**File:** `backend/TaskManager.API/Controllers/UsersController.cs`

**Goal:** Close a privilege-escalation gap surfaced while verifying AUTH01.3 live: `POST /api/users` only inherited AUTH01.2's global `RequireAuthenticatedUser()` fallback policy, so any authenticated user — not just admins — could create new user accounts, including setting `isAdmin: true` on the new account. A regression introduced by AUTH01.2 (before it, nothing was authenticated at all, so the gap didn't exist in practice).

**Acceptance criteria:**
- `POST /api/users` (`CreateUser` action) requires the `Admin` role — e.g. `[Authorize(Roles = "Admin")]` applied at the action or controller level, matching the pattern already established in `ApiTokensController` (AUTH01.3) — no new plumbing needed since `JwtTokenGenerator` (AUTH01.1) already stamps a `ClaimTypes.Role` claim derived from `isAdmin`
- A non-admin authenticated user's valid JWT gets `403` on `POST /api/users`
- An admin user's valid JWT still succeeds unchanged on `POST /api/users`, including creating a user with `isAdmin: true`
- `GET /api/users` and `GET /api/users/{id}` are explicitly reviewed as part of this ticket, but changing them is not mandatory — a product decision not made anywhere else in the planning docs. The implementer must decide one way or the other and record the reasoning, so the decision isn't silently made by omission
- Verified live: non-admin JWT → `403` on `POST /api/users`; admin JWT → unchanged `200`/`201` behavior on `POST /api/users`; whatever decision is made on the two `GET` endpoints is verified against both an admin and a non-admin JWT

Added `[Authorize(Roles = "Admin")]` to just the `CreateUser` action (not the whole controller). **Resolution on `GET /api/users` and `GET /api/users/{id}`: left open to any authenticated user, deliberately not admin-restricted.** Reasoning: `useUsers()` (the shared frontend hook backing this endpoint) is the lookup source for assignee pickers in `TaskForm`/`AssigneeRow` (DS04, U02.4) and for the project-membership "add member" search PM02.2 is about to build — none of those are admin-only surfaces, they're used by any regular logged-in user picking a teammate. Restricting the list would break those flows; only *writing* new user accounts is the privilege-sensitive operation this ticket was about. A code comment on `GetUsers` documents this reasoning in place.

Verified live: non-admin JWT → `403` on `POST /api/users`, including an attempted `isAdmin: true` self-escalation payload; admin JWT → unchanged `201` behavior on `POST /api/users` including `isAdmin: true`; both `GET /api/users` and `GET /api/users/{id}` → `200` for a non-admin JWT (confirming the deliberate no-change decision).

**Discovered along the way:** implementing this ticket surfaced a related bootstrapping gap — a fresh deployment now has no way to create the very first admin account, since `DataSeeder` only seeds a non-login "system" placeholder and every other user-creation path requires an already-authenticated admin. Out of scope for this ticket (existing test admin/non-admin accounts from earlier AUTH01 tickets were used for verification instead), flagged separately as a follow-up task.

---

## AUTH02 Tickets — Frontend: Login & Session

**Epic:** Frontend: Login & session — ✅ Done (Sprint 4, MVP-Required)

| Ticket | Title | Status |
|---|---|---|
| AUTH02.1 | Login screen + axios interceptor | ✅ Done |
| AUTH02.2 | `AuthContext`, route guard, logout | ✅ Done |
| AUTH02.3 | Backend: One-time `/setup` endpoint | ✅ Done |
| AUTH02.4 | Frontend: Setup screen | ✅ Done |

> AUTH02.3/AUTH02.4 close the bootstrap-admin gap discovered during AUTH01.5: once `POST /api/users` requires the `Admin` role, a fresh deployment has no way to create the very first admin account (`DataSeeder` only seeds a non-login "system" placeholder).

---

### AUTH02.1 — Login screen + axios interceptor ✅

**Files:** `frontend/packages/web/src/screens/LoginScreen/` (new), `frontend/packages/shared/src/ApiClient.ts`

**Acceptance criteria:**
- `LoginScreen` renders a username/password form styled with existing design tokens (`bg-surface-overlay`, `bg-primary-800`, etc. — no raw Tailwind colors, per the established DS0x convention)
- On submit, calls the AUTH01.1 login endpoint; on success stores the returned JWT (`localStorage`, consistent with the existing `tm:theme` pattern) and navigates into the app
- On failure, renders an inline error message (not silently swallowed — matches the BUG-05 fix pattern) without leaking whether the username or password was wrong
- `ApiClient.ts`'s axios instance gets a request interceptor that attaches `Authorization: Bearer <token>` from storage to every outgoing request automatically — no call site needs to pass it manually
- Verified in-browser: correct credentials → redirected into the app; wrong credentials → inline error, stays on login; a subsequent API call (e.g. loading projects) succeeds with the token attached

New `frontend/packages/web/src/screens/LoginScreen/` (`LoginScreen.tsx` + `.styles.ts` + barrel, matching the `ProjectSelectorScreen`/folder convention): a centered card styled with the same tokens and `panelGlow` animation `CreateProjectModal` uses, username/password fields, inline error box on failure (`Invalid username or password.` — no distinction between unknown-user and wrong-password, matching the backend's own 401 non-distinction from AUTH01.1). `@taskmanager/shared`'s `ApiClient.ts` gained: a `login()` call (`POST /api/auth/login`), `LoginRequest`/`LoginResponse` types mirroring the backend's `LoginCommand`/`LoginResult`, and `getStoredToken`/`setStoredToken`/`clearStoredToken` helpers around a new `tm:token` localStorage key (same pattern as the existing `tm:theme` key) — plus a request interceptor on the shared axios instance that attaches `Authorization: Bearer <token>` to every outgoing call automatically, so no call site changes. `App.tsx` gained a minimal token-presence gate as a stopgap, replaced by AUTH02.2's real `AuthContext` below.

Verified live in-browser against the running dev server + API: wrong password → 401 from the API, inline error shown, stays on login; correct credentials → 200, redirected straight into `ProjectSelectorScreen` showing real project data (confirming the follow-up `GET /api/projects` succeeded — since AUTH01.2 requires auth on every endpoint, a 200 here is only possible if the interceptor attached a valid token); reloading the page with the token still in `localStorage` stays logged in rather than bouncing back to the login screen.

---

### AUTH02.2 — AuthContext, route guard, logout ✅

**Files:** `frontend/packages/web/src/App/AuthContext.tsx` (new), `frontend/packages/web/src/App/App.tsx`

**Acceptance criteria:**
- `AuthContext` exposes `{ user: { id, username, isAdmin } | null, logout: () => void }`, populated by decoding the stored JWT (or a `GET /api/auth/me` call, implementer's choice) on app load
- Any screen rendered while `user` is `null` redirects to `LoginScreen` instead of rendering — no protected screen is reachable without a valid session
- A logout action (in `FloatingPill`, alongside the existing theme toggle) clears the stored token and `AuthContext` state, returning the user to `LoginScreen`
- An expired/invalid token (401 response from any API call) triggers the same redirect-to-login flow as a missing token
- Verified in-browser: reloading the app with a valid token in storage lands directly on the app (not login); clearing storage and reloading lands on login; clicking logout clears the session and redirects

New `frontend/packages/web/src/App/AuthContext.tsx`: `AuthProvider`/`useAuth`, decoding the stored JWT client-side (no `/api/auth/me` call — the token already carries everything needed) via a small base64url `decodeJwt` helper, checking the `exp` claim against `Date.now()` to treat an expired token as no session. Exposes `{ user, signIn(token), logout() }` — `signIn` (called by `LoginScreen` after a successful `login()` API call) both persists the token and derives `user`, `logout` clears both. `App.tsx` restructured into `AuthProvider` → `AppContent` (the single choke point: `if (!user) return <LoginScreen />`, otherwise mounts `ProjectProvider`/`AppShell` — no protected screen exists outside this branch) → `App`. For the 401-triggered case, `@taskmanager/shared/src/ApiClient.ts` gained a response interceptor that dispatches a `tm:unauthorized` window event on any `401` *except* from `/api/auth/login` itself (a wrong-password attempt is an expected, unrelated 401, not a session going invalid); `AuthContext` listens for that event and clears the session the same way `logout()` does. `FloatingPill` gained a `LogOut`-icon button (reusing existing `iconButton`/`divider` styles, no new styles needed) wired to `onLogout={logout}` from `AppShell`.

Verified live via the browser preview against the running dev server + API: reloading with a valid token in `localStorage` lands directly on the app; clearing `localStorage` and reloading lands on login; clicking the new Log out button clears `tm:token` and redirects to login; tampering with the stored token (simulating expiry/invalidity) and triggering any API call (clicking into a project) produces real `401`s from the backend and the app automatically bounces to the login screen via the `tm:unauthorized` event path, not just the manual logout path.

---

### AUTH02.3 — Backend: One-time `/setup` endpoint ✅

**File:** `backend/TaskManager.API/Controllers/AuthController.cs` (extended), `backend/TaskManager.Application/Auth/Commands/SetupAdminCommand.cs` (new)

**Goal:** Close the bootstrap-admin gap: since AUTH01.5, `POST /api/users` requires an existing admin, so a fresh deployment (empty DB) has no way to create the very first one.

**Acceptance criteria:**
- `POST /api/auth/setup` — `[AllowAnonymous]`, accepts `{ username, password }`, creates the first `AppUser` with `IsAdmin = true` via the existing `IPasswordHasher`
- Guard against the endpoint ever creating a second admin: checks for an existing `IsAdmin = true` user before creating; if one exists, returns `409 Conflict` and creates nothing. Concurrent double-submission (two simultaneous requests against a fresh DB) must still only ever result in exactly one admin — enforced at the DB level (unique constraint / transaction), not just an application-level check-then-create that a race can slip through
- `GET /api/auth/setup/status` — `[AllowAnonymous]`, returns whether setup is still required (i.e. whether any admin exists yet), so the frontend knows whether to show the setup screen at all
- Successful setup returns a JWT immediately (same shape as AUTH01.1's login response) — no separate login step needed right after setup
- Verified: fresh DB → setup succeeds, returns a valid JWT, new user has `isAdmin: true`; calling `/setup` again afterward → `409`, no second user created; concurrent double-submit test on a fresh DB → exactly one admin created

`AuthController` gained `POST /api/auth/setup` and `GET /api/auth/setup/status` (both `[AllowAnonymous]`), backed by new `SetupAdminCommand`/`SetupAdminCommandHandler` and `GetSetupStatusQuery` in `TaskManager.Application/Auth/`. The setup request is only `{ username, password }` — `AppUser` also needs `displayName`/`email`, so the handler synthesizes `displayName = username` and `email = "{username}@localhost"` (mirroring `DataSeeder`'s existing `system@localhost` placeholder convention). Race-safety is enforced at the DB level, not just app-level check-then-create: a new Postgres **partial unique index** (`IX_Users_SingleAdmin` — `UNIQUE ("IsAdmin") WHERE "IsAdmin" = true`, migration `AddSingleAdminConstraint`) means at most one `IsAdmin = true` row can ever exist. `IUserRepository.TryCreateFirstAdminAsync` (Infrastructure) does the insert and catches the resulting `DbUpdateException` if a concurrent request already won, translating it to a clean `false` — keeping EF-specific exception handling out of the Application layer; `SetupAdminCommandHandler`'s own `ExistsAdminAsync` check beforehand is purely a fast-path, not the correctness guarantee. Successful setup returns the same `LoginResult` shape as AUTH01.1's login (token + user), so the frontend can log straight in.

Verified live: on the real dev DB (an admin already existed at the time) → `409`, no data touched. On an isolated throwaway Postgres container (fresh migrations, only the seeded `system` placeholder present): `GET /setup/status` → `setupRequired: true`; `POST /setup` → `200` + valid JWT + `isAdmin: true`; `GET /setup/status` again → `false`; repeat `POST /setup` → `409`, no second user. **Concurrency test:** reset the throwaway DB to zero admins and fired 10 truly simultaneous `POST /setup` requests (`curl ... &` × 10, `wait`) — exactly one `200` (the rest `409`), confirmed against the DB directly (`SELECT ... WHERE "IsAdmin" = true` → exactly one row). The real dev DB and its existing project data were untouched throughout.

---

### AUTH02.4 — Frontend: Setup screen ✅

**File:** `frontend/packages/web/src/screens/SetupScreen/` (new), `frontend/packages/web/src/App/App.tsx`

**Acceptance criteria:**
- On app load, before rendering `LoginScreen`, checks AUTH02.3's setup-status endpoint; if setup is still required, renders `SetupScreen` instead
- `SetupScreen` — username/password + confirm-password form, styled with existing design tokens, calls `POST /api/auth/setup`
- On success: stores the returned JWT and navigates directly into the app, skipping a separate login step
- On failure (e.g. another admin was created in the gap between the status check and submit): inline error, falls back to `LoginScreen` rather than retrying setup
- Setup screen is permanently unreachable once an admin exists — re-checks status on load rather than trusting a cached/stale flag, so direct URL navigation can't reopen it
- Verified in-browser: fresh DB → app loads straight into `SetupScreen`; completing it logs in immediately; reloading afterward (or a second browser/session) shows `LoginScreen`, never `SetupScreen`

New `frontend/packages/web/src/screens/SetupScreen/` (`SetupScreen.tsx` + `.styles.ts` + barrel), visually matching `LoginScreen`'s card but with an added confirm-password field and inline "Passwords don't match." hint (submit stays disabled until they match and the password is ≥8 chars). `@taskmanager/shared`'s `ApiClient.ts` gained `getSetupStatus()`, `setupAdmin()`, and an `isConflictError()` helper (wraps `axios.isAxiosError(...) && status === 409`, keeping axios itself out of the `packages/web` dependency tree). `App.tsx`'s `AppContent` gained a `setupRequired` state, checked via a `useEffect` keyed on `user` (`[user]` deps, not mount-once) — so it re-fires on every transition into the unauthenticated state, including after logout, not just the very first page load; while `null` (checking) a brief themed "Loading…" screen shows rather than flashing blank. `SetupScreen` accepts an `onSetupUnavailable` callback — on a `409` from `POST /api/auth/setup` (AUTH02.3's DB-level guard rejecting a losing race), it calls that instead of showing a retry error, flipping `AppContent` straight to `LoginScreen` without an extra status round-trip. On success, `SetupScreen` calls the same `AuthContext.signIn(token)` AUTH02.2 built for `LoginScreen` — no separate login step needed.

Verified live via the browser preview against an isolated throwaway Postgres container (not the real dev DB): fresh DB → app loads straight into `SetupScreen` (not `LoginScreen`); typing mismatched passwords shows the inline hint and keeps submit disabled; completing setup logs straight into `ProjectSelectorScreen` with no separate login step; clearing storage and reloading shows `LoginScreen`, never `SetupScreen` again, since an admin now exists; logging in normally with the account created via setup succeeds. Also confirmed against the real dev DB (an admin already existed): `setup/status` correctly reported `setupRequired: false` — no regression.

---

## PM01 Tickets — Backend: Project Membership

**Epic:** Backend: Project membership — ✅ Done (Sprint 4, MVP-Required)

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| PM01.1 | `ProjectMember` entity + migration | ✅ Done | — |
| PM01.2 | Auto-assign creator as member | ✅ Done | PM01.1, AUTH01.2 |
| PM01.3 | CQRS + controller for add/remove/list members | ✅ Done | PM01.1 |

---

### PM01.1 — ProjectMember entity + migration

**File:** `backend/TaskManager.Domain/Entities/ProjectMember.cs` (new), EF migration

**Goal:** A real `Project` ↔ `AppUser` many-to-many relationship — doesn't exist today (only per-task `AssignedUserId`/`SecondaryAssigneeId` do).

**Acceptance criteria:**
- `ProjectMember` join entity: `ProjectId`, `UserId`, `JoinedAt` — flat membership, no per-project role field (per user direction: reuse global `AppUser.IsAdmin` for admin powers; full RBAC deferred, see [ROADMAP.md](ROADMAP.md#post-mvp-backlog))
- Unique constraint on `(ProjectId, UserId)` — a user can't be added to the same project twice
- `Project.AddMember(userId)` / `Project.RemoveMember(userId)` domain methods, mirroring the existing `Project.AddEpic`/`RemoveEpic` pattern style
- EF configuration + migration verified against a live PostgreSQL container, cascade-deletes `ProjectMember` rows when the parent `Project` is deleted

**Done** — `ProjectMember` entity (`Id`, `ProjectId`, `UserId`, `JoinedAt` — no `AuditInfo`, mirrors `ApiToken`'s flat-properties style since membership isn't "renamed"/"updated", just added/removed). Constructor is only ever meant to be called via `Project.AddMember`, not directly. `Project` gained a `Members` collection plus `AddMember(Guid userId)`/`RemoveMember(Guid userId)` — per the ticket's literal signature (raw `userId`, not a pre-built entity like `AddPhase(ProjectPhase)`), the aggregate root constructs the `ProjectMember` row internally since there's nothing else the caller needs to supply. Both are idempotent (`AddMember` no-ops if already a member; `RemoveMember` no-ops if not found), matching `AddPhase`/`RemovePhase`'s existing idempotency style. EF migration `AddProjectMember`: `Cascade` delete from `Projects`, `Restrict` from `Users` (consistent with `Project.CreatedByUserId`/`ApiToken.CreatedByUserId`), unique index on `(ProjectId, UserId)` — no navigation property either side (same no-nav style as `ApiToken.CreatedByUserId`).

Verified against a live PostgreSQL container: migration applies cleanly to both the real dev DB (schema-only change, no data touched) and a fresh isolated throwaway container. On the throwaway container — created a real project via the API, then inserted `ProjectMember` rows via direct SQL (no API surface exists yet for this — that's PM01.3): a duplicate `(ProjectId, UserId)` insert correctly fails with a unique-constraint violation; deleting the `Project` row directly cascades to remove its `ProjectMember` rows (confirmed `0` remaining). The `AddMember`/`RemoveMember` domain methods themselves aren't yet exercised through any live path (no test project exists in this repo, and no CQRS/API calls them yet) — that happens for real in PM01.2 (auto-assign creator) and PM01.3 (API surface); they mirror `AddPhase`/`RemovePhase`'s already-proven pattern closely enough that this is a reasonable scope boundary for this ticket specifically.

---

### PM01.2 — Auto-assign creator as member

**File:** `backend/TaskManager.Application/Projects/Commands/CreateProjectCommand.cs`

**Goal:** Whoever creates a project is automatically on it — no manual "add yourself" step.

**Acceptance criteria:**
- `CreateProjectCommandHandler` reads the current authenticated user's id (from the JWT/`ApiToken` principal established in AUTH01.2) and calls `Project.AddMember(currentUserId)` as part of the same creation transaction
- A project can never be created with zero members — the creator is always the first row in `ProjectMember` for that project
- Verified: creating a project via the API while authenticated as User A results in User A appearing in that project's member list immediately, with no separate API call

**Done** — Added a new `ICurrentUserService` interface in the Application layer (`Guid UserId { get; }`). Added `CurrentUserService` in the API layer to implement it. It reads the user id from the JWT claim, or the `ApiToken` header claim if no JWT is present. This matches the exact claim-check logic `ApiTokensController` already used. Registered both `AddHttpContextAccessor()` and the new service in `Program.cs`.

`CreateProjectCommand` no longer accepts `CreatedByUserId` from the client at all. The handler now injects `ICurrentUserService` and uses its `UserId` for both the `Project` constructor and a new `project.AddMember(ownerId)` call, before `SaveChangesAsync`. One `SaveChanges` call writes both the project row and the member row — this is already one transaction.

Deleted `WellKnownUsers.cs` — it was only used by the old fallback logic and is now dead code. Fixed two stale comments in `DataSeeder.cs` that referenced it or "before real auth is wired up." Also refactored `ApiTokensController` to use the new shared service instead of its own private `GetCurrentUserId()` method — removes duplicate claim-parsing logic, no behavior change.

Frontend: removed `createdByUserId` from `CreateProjectRequest` in `ApiClient.ts`, and removed the hardcoded system-user GUID from `CreateProjectModal.tsx`'s `createProject()` call.

Verified live against an isolated throwaway Postgres container (not the real dev DB). Created a real user via `/api/auth/setup`. Created a project with that user's JWT and no owner field in the request body. The returned `createdByUserId` matched the real user's id. A direct SQL query showed exactly one `ProjectMembers` row for that project, with a matching `UserId`. Created two more projects and ran a left-join query for any project with zero members — zero rows came back. Also verified through the real browser UI: logged in as the test user, created a project through `CreateProjectModal`, confirmed the request succeeded with no `createdByUserId` sent, and confirmed the member row appeared in the database.

---

### PM01.3 — CQRS + controller for add/remove/list members

**File:** `backend/TaskManager.Application/Projects/Members/` (new CQRS folder), `backend/TaskManager.API/Controllers/ProjectsController.cs` (extended) or a new `ProjectMembersController`

**Acceptance criteria:**
- `POST /api/projects/{projectId}/members` — adds an existing `AppUser` to the project (by `userId`); `404` if the project or user doesn't exist; no-op-safe (or `409`) if already a member
- `DELETE /api/projects/{projectId}/members/{userId}` — removes a member; a project's creator can also be removed (no special protection for MVP, per flat-membership scope)
- `GET /api/projects/{projectId}/members` — returns the member list as `AppUserDto[]`
- All three endpoints require authentication (inherits the global `[Authorize]` from AUTH01.2)
- Verified via the API directly: add, list (shows the added member), remove, list again (member gone)

**Done** — Extended `ProjectsController` with three new routes (no new controller). Added `IProjectRepository.GetWithMembersAsync` (loads a tracked project with `Members` included, needed since `Project.AddMember`/`RemoveMember` mutate that collection). Added `IUserRepository.GetByProjectAsync` (a join query, since `ProjectMember` has no navigation to `AppUser`). New CQRS folder `Projects/Members/`: `AddProjectMemberCommand`, `RemoveProjectMemberCommand`, `GetProjectMembersQuery`. Add returns `200` with the member's `AppUserDto` (not `204`) — matches this app's convention for idempotent state-set operations like `SetActive`/`SetAdmin`. No new migration was needed; the `ProjectMembers` table already existed.

**Found and fixed a real bug during live verification, not just an implementation detail.** The first `POST /members` call failed with a `DbUpdateConcurrencyException`. The real SQL EF Core generated was `UPDATE "ProjectMembers" ... WHERE "Id" = @p3` — an UPDATE for a brand-new row, not an INSERT. Root cause: `ProjectMember` is the only entity in this app ever added via a collection-navigation call (`Project.AddMember`) on an *already-tracked, existing* parent, rather than an explicit repository `.Add()` call. EF Core's default convention for `Guid` primary keys assumes the value might be database-generated. Since `ProjectMember.Id` is set client-side in the constructor before EF ever sees it, EF read the already-set key as "this must already exist" and marked the entity `Modified` instead of `Added`. Fixed by adding `.Property(m => m.Id).ValueGeneratedNever()` to `ConfigureProjectMember` in `AppDbContext.cs` — this is the standard, documented EF Core fix for this exact scenario, not a workaround. No migration was needed for the fix either; it's a client-side metadata change with no schema impact (confirmed by generating a migration and finding its `Up()`/`Down()` methods both empty, then removing it). This same landmine would have hit any future code calling `Project.AddPhase`/`AddTask` in the same way — those domain methods exist but have never actually been exercised this way anywhere in the app before now.

Verified live against an isolated throwaway Postgres container (not the real dev DB). Created two real users and a project. Added the second user as a member — confirmed by direct SQL immediately (this is the check that caught the bug above). Listed members — both users appeared. Re-added the same user — same `200` response, still exactly two rows, no duplicate. Removed the second user — `204`, list showed only the first. Removed the project's own creator — `204` succeeded with no special protection, and the project itself still existed afterward with `createdByUserId` unchanged, only the membership row gone. Checked all three routes against a nonexistent project id — `POST` and `DELETE` both `404`; `GET` returns `200` with an empty list, matching the same no-existence-check convention `GetPhasesByProjectQuery` already uses (a wording slip in this ticket's own verification checklist said "404" here — the actual, intentional, precedent-matching behavior is `200` + empty list). `POST` with a nonexistent `userId` on a real project — `404`. A call with no `Authorization` header — `401`. Also re-confirmed the real dev DB starts clean with no migration prompt, since the fix has no schema impact.

---

## PM02 Tickets — Frontend: Project Membership UI

**Epic:** Frontend: Project membership UI — ✅ Done (Sprint 4, MVP-Required)

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| PM02.1 | Members panel (list) | ✅ Done | PM01.3 |
| PM02.2 | Add/remove member flow | ✅ Done | PM02.1 |
| PM02.3 | Restrict task assignment to project members | ✅ Done | PM01.3 |

---

### PM02.1 — Members panel (list)

**File:** `frontend/packages/web/src/features/project/MembersPanel/` (new)

**Goal:** Membership lives on the project itself, per user direction — not tucked away in the admin panel.

**Acceptance criteria:**
- Reachable from within a selected project (e.g. an entry point on `BoardScreen`, alongside the `ViewToggle`) — exact placement is implementer's call, but must not require going through the admin panel
- Lists current members using the existing `AssigneeAvatar` component from `@taskmanager/ui`, with usernames
- Uses design tokens throughout — no raw Tailwind colors, consistent with the rest of the app
- Verified in-browser: opening the panel for a project shows its creator (from PM01.2) as a member with no manual setup

**Done** — New `frontend/packages/web/src/features/project/MembersPanel/` (`MembersPanel.tsx` + `.styles.ts` + `index.ts` barrel, matching the existing feature-folder convention). Built as a right-side slide-in drawer, structurally cloned from `TaskDetailPanel.tsx` — backdrop, `fixed inset-y-0 right-0` panel, `Escape`-key close, body-scroll lock, and the existing `drawerIn` keyframe — since it's the only existing "panel that isn't a centered modal" in the app, and the natural fit anchored to `BoardScreen`'s header. `BoardScreen` gained a "Members" button as a sibling of "New Task" in the header, next to `ViewToggle`, guarded by `{selectedProjectId && ...}` the same way. Members are listed via `AssigneeAvatar` + username, backed by a new `useProjectMembers(projectId)` hook — an exact structural clone of `usePhases`/`useTasks` (`{ data, isLoading, error, refetch }`, read-only, mutations handled by the component per this codebase's established convention). New `ApiClient.ts` functions `getProjectMembers`/`addProjectMember`/`removeProjectMember` added alongside `archiveProject`/`restoreProject` (same tier — project sub-resource actions), reusing `AppUserDto` directly — no new DTO needed since PM01.3's endpoints already return it. All styling uses existing design tokens only (`bg-surface-raised`, `border-border`, `text-text-muted`, etc.) — no raw Tailwind colors.

---

### PM02.2 — Add/remove member flow

**File:** `frontend/packages/web/src/features/project/MembersPanel/` (extends PM02.1)

**Acceptance criteria:**
- "Add member" control: searches/lists existing `AppUser`s via `useUsers()`, excluding users already on the project
- Selecting a user calls PM01.3's add-member endpoint and the panel updates without a full page reload
- Each member row (except arguably the current viewer, implementer's call) has a remove action calling the remove-member endpoint, with the list updating immediately
- API errors on add/remove are shown inline, not silently swallowed
- Verified in-browser: add a second user → appears in the list and is immediately eligible to vote (cross-check against PRI01.3 once that ships); remove a member → disappears from the list

**Done** — "Add member" is an expandable section (toggle button → text filter input + a wrapping grid of `AssigneeAvatar` buttons for every non-member user, filtered client-side against `displayName`/`username`). Clicking an avatar calls `addProjectMember` immediately and refetches — no separate submit step, matching the AC's literal "selecting a user calls the add-member endpoint" more directly than a `<select>`+button would have. Reused `AssigneeAvatar` (already mandated for the member list) instead of inventing a second picker UI language. Each member row has its own `busy`/`error` state and a `run(action)` wrapper cloned from `UserManagementScreen.tsx`'s `UserRow` — the closest existing precedent for "list row with an inline mutating action and inline error" — calling `removeProjectMember` then `refetch()`, with `getErrorMessage(e, fallback)` (already exported from `@taskmanager/shared`, already used by `UserManagementScreen`) surfacing failures inline under that row, not swallowed. **Self-removal is allowed, no special-casing, no confirm dialog** — the backend has no special protection for self or the creator (per PM01.3), and no UI in this app gates project access by membership today, so this is a deliberate scope decision, not an oversight. No confirm-dialog pattern exists anywhere else in this codebase for a destructive action either.

Verified live via the browser preview against the real dev API and DB (no throwaway container needed — this is a pure frontend change against already-shipped, already-verified PM01 endpoints). `npx tsc -p tsconfig.app.json --noEmit` on `packages/web` — zero errors. In-browser: opened Members on project "22222" (predates PM01, no members) → "No members yet." shown correctly; expanded Add member, filter grid correctly showed only non-member users; clicked an avatar → `POST /members` → `200`, member appeared immediately, no reload; clicked remove → `DELETE /members/{id}` → `204`, member disappeared immediately, list returned to the empty state without crashing. Opened Members on project "12313" (created after PM01.2 shipped) → its creator `rodrap50` was listed automatically with zero manual setup, confirming the PM01.2 cross-check. Confirmed dark/light token resolution via computed styles (`backgroundColor` correctly resolved `--color-surface-raised` in both themes after toggling). Confirmed the inline-error path fires correctly: a stale-session `POST /api/projects` 500 (an unrelated, pre-existing environment issue — a JWT for a since-deleted test user, not a PM02 defect) rendered inline in `CreateProjectModal`'s existing error box rather than failing silently, confirming this app's general inline-error convention (which `MembersPanel` reuses via `getErrorMessage`) works correctly end-to-end.

---

### PM02.3 — Restrict task assignment to project members

**File:** `backend/TaskManager.Application/Tasks/Validators/*`, `backend/TaskManager.Application/Common/Behaviors/ValidationBehavior.cs`, `frontend/packages/web/src/features/tasks/TaskForm.tsx`, `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx`, `frontend/packages/web/src/screens/BoardScreen/BoardScreen.tsx`

**Goal:** Ad-hoc user request, added after PM01/PM02 shipped: `ProjectTask.AssignedUserId`/`SecondaryAssigneeId` could be set to any `AppUser` in the system, with no check tying an assignment to the task's project roster. Restrict both the backend and the assignee pickers to the project's actual `ProjectMember`s.

**Acceptance criteria:**
- Backend rejects `POST /api/tasks` / `PUT /api/tasks/{id}` when `AssignedUserId`/`SecondaryAssigneeId` is set to a real `AppUser` who is not a member of the task's project — `400` with a field-attributed error message, not a `500` or silent success
- A task whose current assignee was later removed from the project must remain editable (title/description/other fields) without that stale assignee tripping validation on every save — only genuine reassignment attempts are checked
- Frontend assignee pickers (`TaskForm` create flow, `TaskDetailPanel` edit mode) only offer current project members as candidates — read-only display of an already-assigned (possibly since-removed) user is unaffected
- Adding/removing a member via the Members panel (PM02.1/PM02.2) is immediately reflected in the assignee pickers, without a page reload
- No retroactive cleanup of tasks already assigned to non-members — enforcement is forward-only

**Done** — Backend: `ValidationBehavior` switched from synchronous `Validate()` to `ValidateAsync()` (needed for DB-backed rules; safe for every existing sync-only validator, confirmed only one behavior is registered in the MediatR pipeline). New `IProjectRepository.IsMemberAsync(projectId, userId)` (`Context.ProjectMembers.AnyAsync(...)`, a lean existence check rather than loading the full `Members` collection). `CreateTaskCommandValidator` gained an async `MustAsync` rule on `AssignedUserId`. `UpdateTaskCommandValidator` gained a single `CustomAsync` block covering both `AssignedUserId` and `SecondaryAssigneeId` in one task lookup, each attributed to its own `PropertyName` in the `400` response — and critically, each field is only checked against the roster **when its requested value differs from the task's current stored value**. This exemption exists because `TaskDetailPanel.handleSave` always resends the task's current assignee on every save, even when unrelated fields change; without the exemption, a task whose assignee was later removed from the project would become permanently un-editable. The existing global exception handler in `Program.cs` already converts a thrown `ValidationException` into `400 { errors: [...] }` — no change needed there.

Frontend: `BoardScreen` now also owns `useProjectMembers(selectedProjectId)` (built in PM02.1) alongside its existing `useUsers()`, since the member roster crossed from single-consumer (`MembersPanel` only) to multi-consumer (`TaskForm` + `TaskDetailPanel`'s picker too) — matching this codebase's existing "2+ consumers get centrally fetched and prop-threaded" rule. `TaskDetailPanel` gained a `members` prop used **only** by the edit-mode picker; the read-only assignee display and its outer `{users.length > 0}` gate stay on the full `users` list unchanged, so a since-removed assignee still displays correctly when not editing. `TaskForm` dropped its own `useUsers()` call entirely in favor of a `members` prop (no read-only mode to preserve there). `MembersPanel` gained an optional `onMembershipChanged` callback, invoked after its own add/remove `refetch()` calls, wired to `BoardScreen`'s own `useProjectMembers` `refetch` — closes a staleness gap where the two independent hook instances (one in `MembersPanel`, one in `BoardScreen`) would otherwise go out of sync with each other.

Verified live against an isolated throwaway Postgres container + a temporary second API/Vite instance (not the real dev DB — this touches task-assignment correctness directly, worth the full isolation). Backend, via curl: create-task with a non-member assignee → `400`; with a member → `201`; update reassigning to a non-member → `400`; **resending the same still-valid assignee alongside an unrelated field change → `200`** (the core regression this ticket had to avoid introducing); removed a member from the project, then resent that now-stale assignee unchanged with a description edit → `200`, not rejected; a genuine reassignment attempt away from the stale value to a different non-member → still `400`; identical behavior confirmed for `SecondaryAssigneeId`; a nonexistent task id with an assignee payload → validator doesn't throw, handler's existing `404` still fires; a plain sync-only validation failure (empty title) still returns `400` correctly, confirming the `ValidationBehavior` async change didn't regress anything. Frontend, in-browser: Create Task's picker showed only current project members, correctly excluding a user removed from the project; added that user back via the Members panel, then reopened Create Task immediately — the newly-added member appeared with no page reload, confirming the staleness fix; `TaskDetailPanel`'s edit-mode picker showed the same members-only set; the task's read-only view continued to correctly display its (at-the-time non-member) assignee's name and avatar. `npx tsc -p tsconfig.app.json --noEmit` — zero errors. All throwaway resources (container, two temporary process instances) stopped and removed afterward; the real dev API and managed preview server were untouched throughout.

---

## DM01 Tickets — Backend: Epic Entity

**Epic:** Backend: Epic entity — ✅ Done (Sprint 3), signed off 2026-08-26

> Full researched plan: [EPIC-ENTITY-PLAN.md](EPIC-ENTITY-PLAN.md). Mirrors `ProjectPhase`'s existing pattern exactly — confirmed free-form (no fixed/hardcoded set), full CRUD per project, phase assignment on a task is optional. Epic is designed the same way.

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| DM01.1 | Domain: `Epic` entity + `Project`/`ProjectTask` associations | 5 | ✅ Done | — |
| DM01.2 | Infrastructure: EF config + migration + dev DB reset | 3 | ✅ Done | DM01.1 |
| DM01.3 | Application: CQRS commands/queries + DTOs + mapping | 5 | ✅ Done | DM01.2 |
| DM01.4 | API: `EpicsController` + `TasksController` wiring | 3 | ✅ Done | DM01.3 |

---

### DM01.1 — Domain: Epic entity + Project/ProjectTask associations

**File:** `backend/TaskManager.Domain/Entities/Epic.cs` (new), `backend/TaskManager.Domain/Entities/Project.cs` (extended), `backend/TaskManager.Domain/Entities/TaskItem.cs` (extended), `backend/TaskManager.Domain/Enums/ProjectScope.cs` (extended)

**Acceptance criteria:**
- `Epic` entity: `Guid Id`, immutable `Guid ProjectId` + `Project?` nav, `string Name`, `string? Description`, `int DisplayOrder`, `AuditInfo Audit`, `IReadOnlyCollection<ProjectTask> Tasks` — same validation rules as `ProjectPhase`'s constructor
- Behavior methods `Rename`, `UpdateDescription`, `Reorder` — identical bodies to `ProjectPhase`'s, each touching `Audit`
- `Project.cs` gains `IReadOnlyCollection<Epic> Epics` + `AddEpic(Epic)` / `RemoveEpic(Guid)`, mirroring `AddPhase`/`RemovePhase`
- `TaskItem.cs` gains `Guid? EpicId` + `Epic?` nav next to the existing `PhaseId`/`Phase` block, plus `AssignToEpic(Guid)` / `RemoveFromEpic()` mirroring `AssignToPhase`/`RemoveFromPhase` — epic assignment is optional, exactly like phase assignment
- `ProjectScope` enum: `Epic` value removed, renumbered explicitly to `Project = 0, DailyTask = 1`
- Verified: `dotnet build` compiles clean across Domain

**Done** — New `Epic.cs` mirrors `ProjectPhase` field-for-field and method-for-method (`Rename`/`UpdateDescription`/`Reorder`, private EF-only parameterless constructor). `Project.cs` gained `_epics`/`Epics`/`AddEpic`/`RemoveEpic`, identical in shape to the existing Phase methods, plus updated class-level XML docs to reflect that "Epic" is no longer a `ProjectScope` value but a sibling child entity. `TaskItem.cs` gained `EpicId`/`Epic` nav + `AssignToEpic`/`RemoveFromEpic`, placed directly next to the existing Phase block. `ProjectScope.cs`: `Epic = 0` removed, `Project` and `DailyTask` renumbered to `0`/`1`; confirmed via grep that no other backend code referenced `ProjectScope.Epic` before making the change. Verified: full-solution `dotnet build` — 0 errors.

---

### DM01.2 — Infrastructure: EF config + migration + dev DB reset

**File:** `backend/TaskManager.Infrastructure/Data/AppDbContext.cs` (extended), new EF migration, `backend/TaskManager.Infrastructure/Repositories/EpicRepository.cs` (new)

**Acceptance criteria:**
- `AppDbContext` gets `DbSet<Epic> Epics` + `ConfigureEpic(modelBuilder)` mirroring `ConfigureProjectPhase` (key, maxlengths, owned `Audit`, `Epic → Tasks` FK with `OnDelete(SetNull)`); `ConfigureProject` gets `HasMany(p => p.Epics)...OnDelete(Cascade)`
- Migration `AddEpicEntity` generated and inspected to confirm it creates the `Epics` table and `Tasks.EpicId` FK column with `SetNull` delete behavior
- **Before dropping the dev DB:** re-verify the plan's original assumption still holds — no real/valuable data has accumulated since 2026-08-20. If it has, write a backfill `UPDATE` for the `ProjectScope` renumbering instead of dropping
- New `IEpicRepository` (`Application/Common/Interfaces/`) + `EpicRepository` mirroring `IProjectPhaseRepository`/`ProjectPhaseRepository` exactly, including `GetByProjectAsync`
- `IUnitOfWork`/`UnitOfWork` gain an `Epics` repository property
- Verified: migration applies cleanly against a live PostgreSQL container; dev DB rebuilt and boots via existing auto-migration in `Program.cs`

**Done** — `AppDbContext` gained `DbSet<Epic> Epics` + `ConfigureEpic` (maxlengths, owned `Audit`, `SetNull` from `Epic → Tasks`); `ConfigureProject` gained `HasMany(p => p.Epics)...OnDelete(Cascade)`. Migration `AddEpicEntity` generated and inspected — confirmed it creates the `Epics` table, `Tasks.EpicId` column, and the `SetNull` FK exactly as specified. New `IEpicRepository`/`EpicRepository` mirror `IProjectPhaseRepository`/`ProjectPhaseRepository` verbatim (including `GetByProjectAsync`); `IUnitOfWork`/`UnitOfWork` gained an `Epics` property.

**The dev-DB assumption check caught something real.** Re-verifying before dropping the DB (per the ticket's own acceptance criterion) found the dev DB was *not* empty — 11 projects (some clearly test data, but others, e.g. "Honey Do List"/"Reno", looking like real content), 12 tasks, and 3 users. Flagged to the user with the actual data shown rather than proceeding on the ticket's stale assumption; user confirmed a full reset was fine anyway. Database was dropped, recreated, and all 5 migrations (including `AddEpicEntity`) reapplied cleanly from scratch. Verified: `dotnet ef database update` applied with no errors; booting the API against the fresh DB showed the existing auto-migration path running correctly and the `system` placeholder user reseeding as expected; confirmed via `\d "Epics"` and `\d "Tasks"` in psql that the table/column/FK/index shapes matched the migration exactly.

---

### DM01.3 — Application: CQRS commands/queries + DTOs + mapping

**File:** `backend/TaskManager.Application/Epics/` (new folder), `backend/TaskManager.Application/Common/DTOs/EpicDto.cs` (new), `ProjectDto.cs`/`ProjectTaskDto.cs` (extended), `MappingExtensions.cs` (extended), `Tasks/Commands/CreateTaskCommand.cs`/`UpdateTaskCommand.cs` (extended)

**Acceptance criteria:**
- `Epics/Commands/CreateEpicCommand.cs`, `UpdateEpicCommand.cs`, `DeleteEpicCommand.cs`, `Epics/Queries/GetEpicsByProjectQuery.cs`, `Epics/Validators/CreateEpicCommandValidator.cs` — same handler bodies, same partial-update pattern, same FluentValidation rules as their Phase counterparts
- `EpicDto` record: `Id, ProjectId, Name, Description, DisplayOrder, CreatedAt, UpdatedAt`
- `ProjectDto` gains `Epics: IReadOnlyList<EpicDto>`; `ProjectTaskDto` gains `EpicId`
- `MappingExtensions`: new `Epic.ToDto()`; `Project.ToDto()` and `ProjectTask.ToDto()` extended to include the new fields
- `CreateTaskCommand` gains `Guid? EpicId = null`; `UpdateTaskCommand` gains `Guid? EpicId` + `bool ClearEpic`, mirroring the existing Phase handling exactly
- Verified: `dotnet build` compiles clean across Application

**Done** — New `Epics/` folder (`CreateEpicCommand`/`UpdateEpicCommand`/`DeleteEpicCommand`/`GetEpicsByProjectQuery`/`CreateEpicCommandValidator`) mirrors the `Phases/` folder handler-for-handler, including the same partial-update semantics on `UpdateEpicCommand` and identical FluentValidation rules on `CreateEpicCommandValidator` (only a `CreateXCommandValidator` exists for Phase either — no Update/Delete validator to mirror). `EpicDto` added with the exact specified shape; `ProjectDto.Epics` and `ProjectTaskDto.EpicId` added; `MappingExtensions` extended for all three. `CreateTaskCommand`/`UpdateTaskCommand` gained `EpicId`/`ClearEpic` exactly mirroring the Phase fields. A temporary placeholder (`null`/`false` literals) was needed in `TasksController`'s positional `UpdateTaskCommand` construction to keep the full solution building between this ticket and DM01.4 (which wires the real request fields) — resolved within the same session, not left in place. Verified: `dotnet build TaskManager.Application` — 0 errors; full-solution build also confirmed green after the temporary patch.

---

### DM01.4 — API: EpicsController + TasksController wiring

**File:** `backend/TaskManager.API/Controllers/EpicsController.cs` (new), `backend/TaskManager.API/Controllers/TasksController.cs` (extended)

**Acceptance criteria:**
- `EpicsController` (`api/Epics` route): `GET project/{projectId:guid}`, `POST`, `PUT {id:guid}` (via `UpdateEpicRequest`), `DELETE {id:guid}` — same auth/response shape conventions as `PhasesController`
- `TasksController`'s `UpdateTaskRequest` gains `EpicId`/`ClearEpic` fields, threaded into `UpdateTaskCommand` construction alongside the existing Phase fields
- Verified via API directly (Scalar/curl): create an epic, list it by project, assign it to a task via `PUT /api/tasks/{id}`, clear it via `clearEpic: true`, delete the epic and confirm the task's `epicId` becomes `null`

**Done** — New `EpicsController` mirrors `PhasesController` verbatim (route shape, response codes, `Created(string.Empty, result)` on create). `TasksController`'s `UpdateTaskRequest` gained real `EpicId`/`ClearEpic` fields, replacing DM01.3's temporary placeholder. Verified live via curl against the real dev DB (post-reset): created a project + epic → `POST /api/epics` → `201`; listed it via `GET /api/epics/project/{id}` → `200`, epic present; created a task and assigned the epic via `PUT /api/tasks/{id}` with `epicId` → task's `epicId` reflected the assignment; cleared it via `clearEpic: true` → `epicId` back to `null`; reassigned it; deleted the epic via `DELETE /api/epics/{id}` → `204`; re-fetched the task → `epicId` correctly `null`, confirming the `SetNull` cascade from DM01.2 fires correctly end-to-end. Test project archived (not left dangling) as cleanup after verification.

---

## DM02 Tickets — Frontend: Epic Entity Consumption

**Epic:** Frontend: Epic entity consumption — ✅ Done (Sprint 3), signed off 2026-08-26

> No dedicated "manage Epics" screen was in the original ticketed scope (read + assign only, matching the exact boundary Phase itself shipped at) — this was expanded mid-epic (DM02.6) after the user pointed out the gap. Epic picker is **scope-gated to `Project`-scope projects only** — DailyTask projects (see DM03) get neither Phases nor Epics.

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| DM02.1 | Shared types + API client functions | 3 | ✅ Done | DM01.4 |
| DM02.2 | `useEpics` hook | 1 | ✅ Done | DM02.1 |
| DM02.3 | `BoardScreen` wiring | 2 | ✅ Done | DM02.2 |
| DM02.4 | Epic picker in `TaskDetailPanel` (scope-gated) | 5 | ✅ Done | DM02.3 |
| DM02.5 | Trim `CreateProjectModal`'s scope options | 1 | ✅ Done | DM02.1 |
| DM02.6 | Epics management panel (Create/Rename/Delete) | — | ✅ Done | DM02.4 |
| DM02.7 | Epic badge color assignment + Kanban badge parity/positioning | — | ✅ Done | DM02.6 |

---

### DM02.1 — Shared types + API client functions

**File:** `frontend/packages/shared/src/ApiClient.ts`

**Acceptance criteria:**
- New types: `EpicDto`, `CreateEpicRequest`, `UpdateEpicRequest`
- `ProjectDto` gains `epics: EpicDto[]`; `ProjectTaskDto` gains `epicId: string | null`; `CreateTaskRequest` gains `epicId?: string`; `UpdateTaskRequest` gains `epicId?: string; clearEpic?: boolean`
- New `getEpicsByProject` / `createEpic` / `updateEpic` / `deleteEpic` functions mirroring the existing Phase functions
- `CreateProjectRequest.scope` / `UpdateProjectRequest.scope?` trimmed from `'Epic' | 'Project' | 'DailyTask'` to `'Project' | 'DailyTask'`
- Verified: `npm run build --workspace=packages/shared` compiles clean

**Done** — All types and CRUD functions added exactly as specified, mirroring the existing Phase equivalents. `packages/shared` has no dedicated build script (consumed as source directly via workspace linking), so verification used this repo's established equivalent: `npx tsc -p tsconfig.app.json --noEmit` on `packages/web`, which type-checks through the full dependency graph. This surfaced a real, expected downstream break — `CreateProjectModal.tsx`'s `SCOPE_OPTIONS` still included `'Epic'`, no longer assignable to the trimmed request type — which is DM02.5's own scope; fixed together in this same pass to keep the build green between tickets.

---

### DM02.2 — useEpics hook

**File:** `frontend/packages/web/src/hooks/useEpics.ts` (new)

**Acceptance criteria:**
- Mirrors `usePhases.ts` exactly (same loading/error/refetch shape), renamed for Epic
- Verified: `npm run build --workspace=packages/web` compiles clean

**Done** — Exact structural clone of `usePhases.ts` (`{ data, isLoading, error, refetch }`). Verified via `tsc --noEmit` — 0 errors.

---

### DM02.3 — BoardScreen wiring

**File:** `frontend/packages/web/src/screens/BoardScreen/BoardScreen.tsx`

**Acceptance criteria:**
- Calls `useEpics(selectedProjectId)` alongside the existing `usePhases` call, folded into the same loading state
- Passes `epics` down to `KanbanBoard`/`ListView`/`TaskDetailPanel`
- Verified in-browser: opening a `Project`-scope project's board fetches epics with no console errors or added loading flicker

**Done** — `BoardScreen` calls `useEpics` alongside `usePhases`, both folded into the single `loading` boolean, and threads `epics` down to all three consumers. `KanbanBoard`/`ListView`/`TaskDetailPanel` all gained an `epics: EpicDto[]` prop (initially unused in `KanbanBoard`, mirroring the same "declared-but-not-yet-consumed" precedent that `phases` already had there). Verified in-browser: opening the project fired `GET /api/epics/project/{id}` → `200` alongside the existing tasks/phases fetches, with no console errors and no visible loading flicker.

---

### DM02.4 — Epic picker in TaskDetailPanel (scope-gated)

**File:** `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx`

**Note — corrected from the original plan:** [EPIC-ENTITY-PLAN.md](EPIC-ENTITY-PLAN.md) was written before DS05/DS06 shipped and describes adding this picker inside `ListView.tsx`'s own embedded detail panel. That panel no longer exists — both Kanban and List view share the one `TaskDetailPanel.tsx` instance via `BoardScreen`. This ticket targeted the real, current file.

**Acceptance criteria:**
- Epic picker block added directly below the existing Phase picker, structurally identical: `None` + one button per epic, calling `updateTask(task.id, { epicId })` / `{ clearEpic: true }`
- **Scope-gated:** the picker (and the Phase picker, per DM03.3) only renders when the task's project scope is `Project` — entirely absent, not just disabled, for `DailyTask`-scope projects
- `features/board/ListView/ListView.tsx` and `features/board/KanbanBoard/KanbanBoard.tsx` — during implementation, check whether either independently renders a phase badge/pill on rows/cards; if so, add an equivalent epic badge for parity
- Verified in-browser: a `Project`-scope task shows both Phase and Epic pickers, assigns/clears correctly (watch network tab for `PUT /api/tasks/{id}` with `epicId`/`clearEpic`); a `DailyTask`-scope task's detail panel shows neither

**Done** — Epic picker added directly below the Phase picker, identical pill-row structure. `TaskDetailPanel` gained a new `projectScope` prop (threaded from `BoardScreen`'s `project?.scope`); the picker renders only when `projectScope === 'Project'` **and** `epics.length > 0` (mirroring the Phase picker's own existing emptiness gate). Checked both board components per the ticket's own instruction: `ListView` already rendered a `phaseTag` badge on its rows, so it gained an equivalent `epicTag`; `KanbanBoard`'s `TaskCard` rendered **no** phase badge at all, so — correctly following the ticket's literal "if so, add parity" rule — nothing was added to Kanban at this point. (This asymmetry was flagged by the user shortly after and fixed as DM02.7, below.)

Verified live in-browser against a real dev instance: a `Project`-scope task's panel showed both pickers; assigning an epic fired `PUT /api/tasks/{id}` with `epicId` set, read-only view updated to the epic's name, `ListView`'s row showed the new badge. To properly test the scope-gate (not just the emptiness check), a `DailyTask`-scope test project was deliberately given an epic via a direct API call bypassing the UI — its task's detail panel correctly showed no Epic section at all, confirming the gate keys off scope, not just whether epics exist. Test data cleaned up afterward.

---

### DM02.5 — Trim CreateProjectModal's scope options

**File:** `frontend/packages/web/src/screens/ProjectSelectorScreen/CreateProjectModal/`

**Acceptance criteria:**
- `SCOPE_OPTIONS` trimmed from `['Project', 'Epic', 'DailyTask']` to `['Project', 'DailyTask']`
- Verified in-browser: the Type selector in the create-project modal no longer offers "Epic"

**Done** — Folded into DM02.1's pass (see above) since it was required there to keep the build green after the shared `scope` type was trimmed. Verified in-browser: the New Project modal's Type selector shows only "Project"/"Daily".

---

### DM02.6 — Epics management panel (Create/Rename/Delete)

**File:** `frontend/packages/web/src/features/project/EpicsPanel/` (new)

**Goal:** Ad-hoc addition, requested after DM01–DM02.5 shipped — the user pointed out that, per the original ticketed scope ("no dedicated manage-Epics screen, read + assign only"), there was **no way to create an Epic anywhere in the UI at all**. This is a real, pre-existing gap Phase already had too (Phase has never had a create/rename/delete screen either) — Epic just surfaced it first. Given a choice between a minimal inline-create-only affordance and a full management panel, the user chose the full panel.

**Acceptance criteria (as scoped in this session):**
- A slide-in "Epics" panel reachable from `BoardScreen`, structurally consistent with the existing `MembersPanel` drawer pattern
- Create (name input + button), inline rename, and delete — real CRUD, not just Create
- Scope-gated to `Project`-scope projects only, same rule as the picker
- Panel's own epic list and `BoardScreen`'s picker list stay in sync without a page reload

**Done** — New `EpicsPanel` (`EpicsPanel.tsx` + `.styles.ts` + `index.ts`), structurally cloned from `MembersPanel`'s slide-in drawer (backdrop, `fixed inset-y-0 right-0`, `Escape`-close, body-scroll lock, `drawerIn` keyframe). Create is a toggle → name input → `createEpic()` call. Rename is per-row inline: a pencil icon swaps the row into an editable input, `Enter`/checkmark saves via `updateEpic()`, `Escape`/X cancels. Delete is a trash icon with no confirmation dialog — deliberately matching `MembersPanel`'s own no-confirm convention for member removal, since no destructive-action confirm pattern exists anywhere else in this app either. New "Epics" button added to `BoardScreen` next to "Members", gated by `project?.scope === 'Project'`. `EpicsPanel` gained an `onEpicsChanged` callback, invoked after every create/rename/delete, wired to `BoardScreen`'s own `useEpics` `refetch` — the same staleness-prevention pattern PM02.3 established for `MembersPanel`/`onMembershipChanged`, since the epic roster is now a two-consumer resource (the panel's own list and the picker).

Verified live against the user's actual running dev instance (not a throwaway container — pure frontend CRUD against already-verified DM01 endpoints): opened the panel on a real project → "No epics yet."; created "Onboarding Revamp" → `POST /api/epics` → `201`, appeared immediately; renamed it via `Enter` → `PUT /api/epics/{id}` → updated in place; deleted it → `DELETE /api/epics/{id}` → `204`, list returned to empty. Zero console errors throughout. Test epic removed afterward so nothing was left in the user's real data.

---

### DM02.7 — Epic badge color assignment + Kanban badge parity/positioning

**File:** `backend/TaskManager.Domain/Entities/Epic.cs` (extended), new EF migration, `backend/TaskManager.Application/Common/DTOs/EpicDto.cs` + `Epics/Commands/CreateEpicCommand.cs` (extended), `frontend/packages/ui/src/components/TaskCard/` (extended), `frontend/packages/web/src/features/board/KanbanBoard/`, `frontend/packages/web/src/features/board/ListView/`, `frontend/packages/web/src/features/project/EpicsPanel/`, `frontend/packages/web/src/constants/colorSwatches.ts` (new)

**Goal:** Two related ad-hoc requests. First, the user noticed `ListView` showed the epic badge but the Kanban board didn't — root cause was that `TaskCard` (Kanban's card component) never rendered a phase *or* epic badge at all; DM02.4 had correctly followed its own "add parity where a phase badge already exists" rule, but Kanban had no phase badge either, so nothing was added there, leaving the two views inconsistent. Second, the user asked for each Epic to be assigned, at creation, a random color from the same palette used for Projects (excluding the owning project's own color), with badges rendered using that color as their background — plus, mid-turn, moving the Kanban badge to the card's top-right corner.

**Acceptance criteria (as scoped in this session):**
- `TaskCard` renders an epic badge, matching `ListView`'s existing badge for parity
- `Epic` gains a persisted color, assigned randomly from the Project color palette at creation, excluding the owning project's current color
- Both the Kanban and List badges render using that color as their background
- Kanban badge is positioned in the card's top-right corner

**Done** — **Kanban parity:** `TaskCard` (`@taskmanager/ui`) gained an `epicName` prop; `KanbanBoard` builds an `epicMap` (mirroring `ListView`'s own) and passes the epic's name through. **Color:** `Epic` domain entity gained a `ColorHex` field, normalized identically to `Project.ColorHex`'s existing `NormalizeColorHex` helper (duplicated into `Epic.cs`, consistent with how this codebase already mirrors small helpers per-entity rather than sharing them). New migration `AddEpicColorHex` — a simple additive nullable column, no data-loss risk (unlike DM01.2's enum-renumbering migration). `EpicDto`/`CreateEpicCommand` extended to carry it through. Frontend: the `COLOR_SWATCHES` array, previously inlined in `CreateProjectModal`, was extracted to a new shared `frontend/packages/web/src/constants/colorSwatches.ts` so both the project-creation picker and the new random-assignment logic draw from one source of truth. `EpicsPanel`'s create flow now calls a `pickRandomEpicColor(excludeHex)` helper that filters the palette against the project's own `colorHex` (threaded down as a new `projectColorHex` prop from `BoardScreen`) before picking randomly, falling back to the full palette in the unreachable edge case where every swatch were somehow excluded. `EpicsPanel` also gained a small color dot next to each epic's name so assigned colors are visible without opening the board. **Badge rendering:** both `TaskCard`'s and `ListView`'s epic badges now apply the epic's color as an inline `backgroundColor` with white text; `TaskCard`'s badge was repositioned from sharing a row with the priority badge to `position: absolute; top: 8px; right: 8px` on the card (card itself gained `position: relative`).

This session's backend change required briefly stopping the user's own live `dotnet run` process (its build output had the DLLs locked) — confirmed with the user first, then restarted after the migration and rebuild completed, per their preference of restarting it themselves. Verified live against the user's real running instance: created 3 epics on a project whose own color was `#6366F1` → got `#F59E0B`, `#8B5CF6`, `#0EA5E9`, three distinct colors, none matching the excluded project color; assigned one to a task and confirmed via computed styles that the Kanban badge is `position: absolute`, `top: 8px`, `right: 8px`, with `background-color: rgb(245, 158, 11)` (`#F59E0B`) and white text, and that `ListView`'s badge for the same task carries the identical color. All test epics and the temporary task assignment were cleaned up afterward, leaving the user's real data untouched.

---

## DM03 Tickets — DailyTask Simplified Todo List

**Epic:** DailyTask projects become a simplified todo list — ✅ Done (Sprint 3), signed off 2026-08-26

> Added mid-planning alongside the Epic entity work since both touch `ProjectScope`/project-type behavior. Confirmed with the user: DailyTask-scope projects show **only** a list view (no Kanban/ViewToggle at all), tasks use a **simple complete/incomplete model**, and DailyTask projects get **no Phases at all** (extending naturally to no Epics either, per DM02.4's scope-gating). This is enforced at the UI layer only, consistent with how other cross-cutting UI rules in this app work (e.g. A01's desktop guardrail) — not enforced server-side. Closes out Phase 5 / Sprint 3 in full (DM01, DM02, DM03 all done).

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| DM03.1 | Hide ViewToggle/Kanban entirely for DailyTask projects | 2 | ✅ Done | — |
| DM03.2 | Simplified Todo list component | 5 | ✅ Done | DM03.1 |
| DM03.3 | Hide Phase picker for DailyTask projects | 2 | ✅ Done | — |

---

### DM03.1 — Hide ViewToggle/Kanban entirely for DailyTask projects

**File:** `frontend/packages/web/src/screens/BoardScreen/BoardScreen.tsx`, `ViewToggle/`

**Goal:** DailyTask-scope projects only ever show a list — no Kanban option exists for them.

**Acceptance criteria:**
- When the selected project's scope is `DailyTask`, `ViewToggle` is not rendered at all — there is only one mode, so there's nothing to toggle
- `viewMode` is forced to the list-equivalent render for DailyTask projects regardless of any stored `localStorage` preference from a different project
- `Project`-scope projects retain the existing toggle behavior, completely unchanged
- Verified in-browser: opening a DailyTask project shows only the list, no toggle control anywhere; opening a `Project`-scope project shows the toggle exactly as before

**Done** — `BoardScreen` derives a new `isDailyTask` boolean from `project?.scope`. `ViewToggle` is wrapped in `{!isDailyTask && ...}`; the Kanban/List render branches gained an `!isDailyTask` guard alongside their existing `viewMode` checks, and a new `isDailyTask` branch renders `TodoList` (DM03.2) unconditionally regardless of the stored `viewMode`. Verified in-browser: opening a DailyTask test project showed no toggle control and a flat list row (not Kanban columns) even after forcing `localStorage['tm:viewMode']` to `'kanban'` and reloading; opening the real `Project`-scope project ("1234") continued to show the toggle and full Kanban columns completely unaffected.

---

### DM03.2 — Simplified Todo list component

**File:** `frontend/packages/web/src/features/board/TodoList/` (new)

**Goal:** A genuinely simpler list for DailyTask projects — checkbox + title, not the full-featured `ListView`.

**Note on the completion model:** reuses the existing `TaskStatus` enum rather than introducing a new field — checking the box calls `transitionTask(id, 'Done')`, unchecking calls `transitionTask(id, 'Backlog')`. This lines up with the existing domain rule (BUG-07/`TaskItem.Transition`) that a `Done` task can only be reopened to `Backlog` — a checkbox toggle is exactly that rule expressed as UI. No backend change needed.

**Acceptance criteria:**
- New `TodoList` component renders tasks as a plain checkbox + title row — no status pills, phase tags, or epic tags (none of which apply to DailyTask projects)
- Priority and due date remain visible (still meaningful for a todo list); implementer's call on exact layout
- Checking the box calls `transitionTask(id, 'Done')`; unchecking calls `transitionTask(id, 'Backlog')`
- `BoardScreen` renders `TodoList` instead of `KanbanBoard`/`ListView` whenever the project's scope is `DailyTask`
- Verified in-browser: checking/unchecking a task updates its status via the API and persists after reload; no Kanban/List toggle, phase, or epic UI appears anywhere on a DailyTask project's board

**Done** — New `TodoList`/`.styles.ts`/`index.ts` (mirroring the existing feature-folder convention). Each row is a checkbox button (checked state derived directly from `task.status === 'Done'`, not the broader `isCompleted` computed field, to avoid conflating with `Cancelled`) plus title, `PriorityBadge`, and a due-date label cloned from `ListView`'s own `formatDue` helper — no status pill, phase tag, or epic tag anywhere. The checkbox's `onClick` stops propagation so it toggles independently of the row's own click-to-open-detail-panel behavior; toggling calls `transitionTask` directly and updates local state optimistically only on success (mirroring `KanbanBoard`'s local-state-synced-from-props pattern), with a per-row inline error (reusing `getErrorMessage`, the same lightweight convention `EpicsPanel`/`MembersPanel` rows already use) on failure rather than silent failure. `BoardScreen` renders `TodoList` whenever `isDailyTask` is true, ahead of the `KanbanBoard`/`ListView` branches.

Verified live against a throwaway DailyTask test project (created directly via SQL rather than through the UI — this session's Claude Browser test tab turned out to be carrying a stale JWT for the since-deleted `devadmin` test account from earlier DM01/DM02 verification, which broke project *creation* specifically since that's the one operation that resolves the current user for `CreatedByUserId`; not a real app bug, confirmed by testing that plain `Project`-scope project creation failed identically with the same stale token). Checking the box fired `POST /api/tasks/{id}/transition` and the DB confirmed `Status` flipped `0 → 3` (`Backlog → Done`); unchecking flipped it back `3 → 0`; both survived a full page reload (confirmed via the checkbox's `aria-label` flipping between "Mark ... complete"/"Mark ... incomplete" after a fresh navigation, not just in-memory state). No Kanban/List toggle, phase, or epic UI appeared anywhere on the test project's board. Test project and task removed afterward.

---

### DM03.3 — Hide Phase picker for DailyTask projects

**File:** `frontend/packages/web/src/features/tasks/TaskDetailPanel.tsx`, `frontend/packages/web/src/features/tasks/TaskForm.tsx` (if it offers a phase field)

**Goal:** DailyTask projects never show phase assignment — "no Phases at all," per user direction.

**Acceptance criteria:**
- `TaskDetailPanel`'s existing Phase picker section is omitted entirely when the task's project scope is `DailyTask`
- `TaskForm`/`CreateTaskModal` — phase field, if offered there, is similarly omitted when creating a task within a DailyTask project
- `Project`-scope projects' Phase picker is completely unaffected — still renders exactly as today
- Verified in-browser: a DailyTask project's task detail panel shows no Phase section at all; a `Project`-scope project's still does, unchanged

**Done** — `TaskDetailPanel` gained a `showPhases = projectScope !== 'DailyTask'` check (mirroring the existing `showEpics` gate from DM02.4), added to the Phase picker's render condition alongside its existing `phases.length > 0` emptiness check. `TaskForm`/`CreateTaskModal` never offered a phase field to begin with — confirmed via grep, nothing to change there.

Verified live: to properly test the scope-gate rather than just the emptiness check, a phase was deliberately inserted directly on the DailyTask test project via SQL — its task's detail panel still showed no Phase section at all. A phase was then added the same way to the real `Project`-scope project ("1234") to confirm the positive case — its task detail panel correctly rendered a Phase section (`None`, since no task was assigned to it), alongside the existing Epic section, completely unaffected. `tsc --noEmit` clean throughout DM03. All test phases and the throwaway DailyTask project were removed afterward, leaving only the user's real "1234" project in the dev DB.

---

## PRI01 Tickets — Backend: Voting & Weighted Scoring

**Epic:** Backend: Voting & weighted scoring — ✅ Done (Sprint 5, MVP-Required), signed off 2026-08-28

> Depends on Sprint 4 in full — needs `ProjectMember` (PM01) to gate who can vote and the current-user principal (AUTH01/02) to attribute votes. Confirmed with the user: voting/weighted scoring applies uniformly to `DailyTask`-scope tasks too, using the exact same formula as `Project`-scope tasks — nothing here is scope-gated (unlike Phase/Epic in DM02/DM03). Backend was already scope-agnostic when originally written; only the frontend display tickets (PRI02/PRI03) needed real changes for `DailyTask`.

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| PRI01.1 | `TaskVote` entity + `Project.CriticalityScore` + migration | 3 | ✅ Done | — |
| PRI01.2 | Weighted score computation | 3 | ✅ Done | PRI01.1 |
| PRI01.3 | Cast/update vote endpoint | 3 | ✅ Done | PRI01.2, PM01.3 |
| PRI01.4 | Admin-settable `CriticalityScore` endpoint | 2 | ✅ Done | PRI01.1 |

---

### PRI01.1 — TaskVote entity + Project.CriticalityScore + migration

**File:** `backend/TaskManager.Domain/Entities/TaskVote.cs` (new), `backend/TaskManager.Domain/Entities/Project.cs` (extended), EF migration

**Acceptance criteria:**
- `TaskVote` entity: `Id`, `TaskId`, `UserId`, `VoteValue` (int, 1–10, validated at the domain/FluentValidation layer), `CreatedAt`/`UpdatedAt`
- Unique constraint on `(TaskId, UserId)` — one editable vote per user per task; casting again updates the existing row rather than inserting a new one
- `Project.CriticalityScore` — int, 1–10, defaults to a documented value (e.g. 5) on project creation, nullable-not-allowed once set
- Migration verified against a live PostgreSQL container; existing `Project`/`ProjectTask` rows get the new column/table without data loss

**Done** — `TaskVote` mirrors `ProjectMember`'s flat-entity pattern (no `AuditInfo`; plain `CreatedAt`/`UpdatedAt`), with an `UpdateValue(int)` method for in-place re-voting. `ProjectTask` gained a `Votes` collection and a `CastVote(userId, voteValue)` aggregate method mirroring `Project.AddMember`. `Project` gained `CriticalityScore` (int) plus a `DefaultCriticalityScore = 5` constant and a validated `SetCriticalityScore(int)` method. `AppDbContext`: `TaskVote.Id` needed the same `ValueGeneratedNever()` fixup as `ProjectMember.Id` (PM01.3), since `CastVote` is invoked via collection-navigation on an already-tracked `ProjectTask`, not an explicit repository `.Add()`. Caught and fixed a real bug while generating the migration: EF's auto-generated `Up()` defaulted existing rows' `CriticalityScore` to `0` instead of `5` — fixed with an explicit Fluent `HasDefaultValue(Project.DefaultCriticalityScore)` before regenerating. Verified against the live dev PostgreSQL container: table/FK/unique-index structure confirmed via `psql`, and both pre-existing real projects backfilled correctly to `CriticalityScore = 5`.

---

### PRI01.2 — Weighted score computation

**File:** `backend/TaskManager.Application/Common/Services/WeightedScoreCalculator.cs` (new) or equivalent domain service

**Goal:** A single, testable place that implements the agreed formula exactly — applies identically regardless of the task's project scope.

**Acceptance criteria:**
- `PriorityNumeric` mapping is exact: Low = 3, Medium = 7, High = 9, Critical = 10
- `AvgUserVote` = ceiling of the arithmetic mean of all `TaskVote.VoteValue` rows for a task (0 votes cast → documented fallback, e.g. treat as the task's own `PriorityNumeric` or a neutral midpoint — implementer flags the choice in the PR/ticket notes since this wasn't specified)
- Final weighted score = `ceiling((PriorityNumeric + AvgUserVote + Project.CriticalityScore) / 3)`, clamped to the `1–10` range
- Rounding is ceiling at every step (never floor/round-to-nearest) — verified with at least one test case where naive rounding would produce a different result than ceiling
- Recalculated and persisted on: vote cast/changed (PRI01.3), task's `Priority` enum changed, and project's `CriticalityScore` changed (PRI01.4)

**Done** — New `WeightedScoreCalculator` static class, placed in the **Domain** layer (`Domain/Services/`, not Application) so `ProjectTask` can call it directly without inverting the Domain→Application dependency direction — the ticket's own "or equivalent domain service" wording allowed this. `ProjectTask` gained a persisted `WeightedScore` property and a `RecalculateWeightedScore(criticalityScore)` method. Wired into `CreateTaskCommand` (recomputes using the real project's `CriticalityScore` rather than the constructor's `DefaultCriticalityScore` placeholder) and `UpdateTaskCommand` (recalculates whenever `Priority` changes). Found and fixed a related correctness gap along the way: `GenericRepository.GetByIdAsync` used a plain `FindAsync`, which never populates collection navigations — so `RecalculateWeightedScore` would silently treat any existing votes as empty on every load. Made `GetByIdAsync` virtual and overrode it in `ProjectTaskRepository` to eagerly `Include(Votes)`. The `AddTaskWeightedScore` migration hit the same EF default-value gotcha as PRI01.1 — hand-added a raw SQL backfill computing each existing task's real score from its `Priority` and parent `CriticalityScore` (same ceiling formula) instead of leaving them at a placeholder `0`. Verified live via the real running API: task creation and priority changes produced hand-verified scores (Medium→7, Critical→9, Low→4); the vote-averaging, ceiling-vs-round-to-nearest divergence, and clamping logic were verified directly against the real `WeightedScoreCalculator` class (no vote-cast endpoint existed yet — that's PRI01.3).

---

### PRI01.3 — Cast/update vote endpoint

**File:** `backend/TaskManager.Application/Tasks/Commands/CastVoteCommand.cs` (new), `backend/TaskManager.API/Controllers/TasksController.cs` (extended)

**Acceptance criteria:**
- `PUT /api/tasks/{taskId}/votes` accepts `{ voteValue: 1-10 }`, authenticated via the current user's principal (no `userId` in the request body — always taken from the token)
- Returns `403` if the current user is not a `ProjectMember` (PM01) of the task's project
- Returns `400` for `voteValue` outside `1–10`
- Casting a vote when one already exists from this user updates it in place rather than erroring
- On success, triggers PRI01.2's recalculation and returns the task's updated weighted score
- Verified: a project member can vote and re-vote; a non-member gets `403`; an out-of-range value gets `400`

**Done** — `PUT /api/tasks/{id}/votes`, taking the voting user's ID from `ICurrentUserService` (never the request body) mirroring `ApiTokensController`'s existing pattern. New `ForbiddenAccessException` type, mapped to `403` in `Program.cs`'s global exception handler — distinct from FluentValidation's `ValidationException` (`400`), since a non-member vote is an authorization failure, not malformed input. `CastVoteCommandHandler` loads the task, checks `IProjectRepository.IsMemberAsync`, calls `ProjectTask.CastVote()`, then `RecalculateWeightedScore()` using the parent project's current `CriticalityScore`. `CastVoteCommandValidator` enforces `1–10` (`400`). Verified live end-to-end: a non-member gets `403`, an out-of-range value gets `400`, a member's vote applies and recalculates correctly, and re-voting updates the existing `TaskVotes` row in place (confirmed exactly one row in the DB after two votes from the same user, not two) — confirming the `ValueGeneratedNever()` fix from PRI01.1 works correctly for this insert path too.

---

### PRI01.4 — Admin-settable CriticalityScore endpoint

**File:** `backend/TaskManager.Application/Projects/Commands/UpdateProjectCommand.cs` (extended) or a dedicated command

**Acceptance criteria:**
- `CriticalityScore` is settable via an admin-only path (`isAdmin` claim required, `403` otherwise)
- Value validated to `1–10`
- Changing it triggers PRI01.2's recalculation for every task in that project (not just newly-touched ones)
- Verified: admin can update; non-admin gets `403`; changing the value visibly shifts affected tasks' weighted scores on next read

**Done** — `PUT /api/projects/{id}/criticality-score`, gated `[Authorize(Roles = "Admin")]`, mirroring `UsersController.SetAdmin`'s exact pattern (a dedicated command rather than folding into `UpdateProjectCommand`, to keep the admin-only authorization concern isolated from the general-purpose update path). New `SetProjectCriticalityScoreCommand` sets `Project.CriticalityScore`, then recalculates and persists `WeightedScore` for every task in the project via a new `IProjectTaskRepository.GetTrackedByProjectAsync` (tracked, `Include(Votes)` — distinct from the existing `AsNoTracking()` `GetByProjectAsync`). `ProjectDto`/mapping gained `CriticalityScore`. Verified live with a throwaway admin JWT (minted locally using the dev-only signing key already checked into `appsettings.Development.json`, since the real admin's credentials weren't known — safe because this handler never resolves the token's user ID, only the `Admin` role claim): non-admin gets `403`, out-of-range gets `400`, and an admin update correctly cascaded to two tasks of different priorities in a single call (Medium 7→5, Critical 10→7, both hand-verified against the formula), confirmed both in the mutation's own response and via an independent follow-up `GET`.

---

## PRI02 Tickets — Frontend: Voting UI

**Epic:** Frontend: Voting UI — ✅ Done (Sprint 5, MVP-Required), signed off 2026-08-28

> Voting/weighted scoring applies to **all** tasks regardless of project scope, per user direction — `DailyTask` tasks are weighted identically to `Project`-scope tasks, no scope-gating (unlike Phase/Epic). Revised 2026-08-26 to correct file paths that drifted since this was first drafted and to add PRI02.4 (`TodoList`, DM03.2). Revised again 2026-08-27 to add PRI02.5, an ad-hoc addition for the previously-missing `CriticalityScore` admin control.

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| PRI02.1 | Voting control component | 5 | ✅ Done | PRI01.3 |
| PRI02.2 | Weighted score display on TaskCard | 2 | ✅ Done | PRI01.2 |
| PRI02.3 | Weighted score display on List view rows | 2 | ✅ Done | PRI01.2 |
| PRI02.4 | Weighted score display on TodoList rows | 2 | ✅ Done | PRI01.2 |
| PRI02.5 | Admin: set project criticality score (interim UI) | 3 | ✅ Done | PRI01.4 |

---

### PRI02.1 — Voting control component

**File:** `frontend/packages/web/src/features/tasks/VoteControl.tsx` (new), wired into `TaskDetailPanel`

**Acceptance criteria:**
- Renders a 1–10 selector, must use design tokens
- Shows the current authenticated user's existing vote (if any) as pre-selected, and the current aggregate
- Selecting a value calls PRI01.3's endpoint immediately and updates the displayed aggregate on success
- API errors surfaced inline, not silently swallowed
- Not scope-gated — renders for tasks in every project scope
- Verified in-browser: casting a vote updates the aggregate; re-voting changes the value in place
- Automatically usable from Kanban, List view, and `TodoList` via the one shared `TaskDetailPanel` instance

**Done** — Discovered a real gap mid-ticket: `ProjectTaskDto` had no way to expose an individual vote value at all, which this ticket's own acceptance criteria requires ("shows the current user's existing vote as pre-selected"). Per user direction, added `Votes: Dictionary<Guid, int>` (userId → voteValue) to the DTO, eagerly loaded wherever tasks are read (`ProjectTaskRepository.GetByProjectAsync`, `ProjectRepository.GetWithChildrenAsync`/`GetAllWithChildrenAsync`). New `VoteControl.tsx`: 1–10 pill selector mirroring the existing Priority picker's styling, pre-selects the current user's vote by looking up their own ID in the `votes` dictionary, shows the live weighted score, fires immediately on click, errors surfaced inline via the existing `getErrorMessage` helper. Wired into `TaskDetailPanel` unconditionally (works in both view and edit mode), not scope-gated, automatically live on Kanban/List/`TodoList` since they share one panel instance. Verified live end-to-end: cast a vote → weighted score recalculated and the pill highlighted; re-voted → updated in place (confirmed exactly one `TaskVotes` row, not two); closed the panel, did a full page reload, reopened it → the vote was still correctly pre-selected from fresh server data.

---

### PRI02.2 — Weighted score display on TaskCard

**File:** `frontend/packages/ui/src/components/TaskCard/TaskCard.tsx`

**Acceptance criteria:**
- `TaskCard` displays the computed weighted score — `Priority` must still be visible too
- Placement: bottom-right corner of the card (top-right already claimed by the epic badge)
- Score uses a consistent visual treatment styled with design tokens
- Verified in-browser: score updates after a vote is cast or `CriticalityScore` changes; no visual overlap with the epic badge

**Done** — New shared, position-agnostic `WeightedScoreBadge` component in `@taskmanager/ui`, extracted immediately since PRI02.3/PRI02.4 need the identical visual treatment. Originally planned to absolutely-position it at the bottom-right corner (mirroring the epic badge's top-right), but caught during implementation that this would collide with the footer's inline assignee avatar, which also sits in that corner — a real overlap the ticket's own acceptance criteria didn't anticipate (it only called out checking against the epic badge). Fixed by folding the score badge into the footer's flex row instead, grouped with the assignee avatar, so they lay out side by side and never overlap. Later, ad-hoc (requested after initial ship), gained a green→yellow→red gradient background computed via HSL interpolation over the 1–10 score range, applied inline (same pattern already used for per-epic/per-project custom colors); badge text switched to a fixed white since the background is now dynamic rather than a theme token. Verified live: across three layout cases (plain task, assignee+due-date, epic+assignee) computed bounding rects confirmed zero overlap in any combination; the gradient math was separately verified against three data points (scores 4/6/9 → `rgb(126,173,31)`/`rgb(173,158,31)`/`rgb(173,62,31)`), each matching hand-computed HSL conversions exactly.

---

### PRI02.3 — Weighted score display on List view rows

**File:** `frontend/packages/web/src/features/board/ListView/ListView.tsx`

**Acceptance criteria:**
- Each list row displays the computed weighted score, visual treatment consistent with PRI02.2's badge
- `Priority` remains visible on the row too
- Verified in-browser: a row's score matches the same task's `TaskCard` score; updates after a vote or `CriticalityScore` change

**Done** — `WeightedScoreBadge` added to each row's `metaRow`, next to `PriorityBadge`. Needed no new styling since the row's existing flex layout absorbed it with zero collision risk (unlike TaskCard's absolute-positioned corners). Verified live: a row's score matched the same task's `TaskCard` score exactly, including the identical gradient color; cast a vote through the row's shared `TaskDetailPanel` → row updated immediately on panel close, no manual reload.

---

### PRI02.4 — Weighted score display on TodoList rows

**File:** `frontend/packages/web/src/features/board/TodoList/TodoList.tsx`

**Acceptance criteria:**
- Each `TodoList` row displays the computed weighted score, visually consistent with PRI02.2/PRI02.3
- `Priority` (already shown via `PriorityBadge`) remains visible alongside it
- Verified in-browser: a `DailyTask` task's row shows the same score an equivalent `Project`-scope task would show; updates after a vote or `CriticalityScore` change

**Done** — Same change as PRI02.3, applied to `TodoList`'s row `metaRow`. Verified live: a `DailyTask`-scope task's row showed a score and gradient color exactly matching an equivalent `Project`-scope task created specifically to cross-check this (both priority `High`, zero votes, default criticality → both `8`, identical `rgb(173,94,31)`); cast a vote → row updated immediately on panel close.

---

### PRI02.5 — Admin: set project criticality score (interim UI)

**File:** `frontend/packages/web/src/screens/admin/ProjectCriticalityScreen/` (new) — a screen under the existing Admin section, wired into `AdminShell.tsx`'s nav

**Goal:** PRI01.4 shipped an admin endpoint for `Project.CriticalityScore` with no frontend consumer anywhere — the only way to change it was a direct API call.

**This is an interim/temporary UI** — see [ROADMAP.md](ROADMAP.md#post-mvp-backlog)'s "Project Settings page (per-project, admin-scoped)" backlog item. When that page is built, this screen's per-project control should migrate there.

**Acceptance criteria:**
- Screen only reachable by admins
- Lists every project with a 1–10 input per row, pre-populated with its current `CriticalityScore`
- Saving calls PRI01.4's endpoint
- Out-of-range/invalid input handled without corrupting state; API errors surfaced inline
- Clearly marked as interim
- Verified in-browser: admin can view/change values and they persist; non-admins never see it; changes cascade to affected tasks' weighted scores

**Done** — Ad-hoc ticket: the user asked where a project's weight is actually set, and it turned out no PRI02 ticket had ever built a frontend control for PRI01.4's endpoint. **First shipped as a small `isAdmin`-gated section embedded in `MembersPanel`** (hidden, not disabled, for non-admins — matching `FloatingPill`'s existing admin-button convention) — fully built and verified working (pre-selected value, cascade to task scores, persisted across reload, invisible to non-admins). The user then redirected: *"We have an Admin Page... add a new page view off admin that lists all the projects."* The `MembersPanel` version was fully reverted, and the control was rebuilt as a new `ProjectCriticalityScreen` under the existing Admin section, wired into `AdminShell`'s nav — inheriting the whole section's existing admin-only gating for free instead of needing its own `isAdmin` check. A table of every project (name, scope, a 1–10 input) mirroring `UserManagementScreen`'s exact table/row pattern. Originally committed on blur; the user reported it "doesn't seem to save," so this was replaced with an explicit **Save** button per row — disabled unless the typed value is a valid, changed 1–10 integer, and re-disabled after a successful save. Along the way, also fixed a related staleness gap by syncing a row's displayed value if the underlying project's score ever changes from elsewhere. Verified live with a throwaway admin JWT: the Save button starts disabled, enables on a real edit, persists correctly on click (confirmed against the server independently), and re-disables at the new value afterward; a non-admin session shows no trace of the "Project Criticality" nav item at all.

**Incident, caught and fixed during PRI03.2's verification:** a rapid-fire round of admin-screen testing for this ticket accidentally left the real "1234" project's `CriticalityScore` at `1` instead of its correct default of `5` — almost certainly a stray test action rather than anything the real user did. Reset back to `5` via the same admin endpoint as soon as it was noticed; the cascade recomputed the project's real task's weighted score back correctly, and all other real projects were confirmed untouched.

---

## PRI03 Tickets — Default Sorting

**Epic:** Default sorting by weighted score — ✅ Done (Sprint 5, MVP-Required), signed off 2026-08-28

> Revised 2026-08-26: corrected stale file paths and extended PRI03.1's scope to include `TodoList` (DM03.2), since `DailyTask` tasks are weighted identically to `Project`-scope tasks per user direction.

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| PRI03.1 | Board/List/TodoList default sort by weighted score | 3 | ✅ Done | PRI01.2 |
| PRI03.2 | Project selector default sort by criticality score | 1 | ✅ Done | PRI01.1 |

---

### PRI03.1 — Board/List/TodoList default sort by weighted score

**File:** `frontend/packages/web/src/features/board/KanbanBoard/KanbanBoard.tsx`, `frontend/packages/web/src/features/board/ListView/ListView.tsx`, `frontend/packages/web/src/features/board/TodoList/TodoList.tsx`

**Acceptance criteria:**
- Within each Kanban column, tasks render sorted by weighted score descending by default
- `ListView`'s prioritized list sorts by weighted score descending by default
- `TodoList`'s rows sort by weighted score descending by default
- Default ordering only — no user-facing sort control added here
- Verified in-browser: seeding tasks with distinct weighted scores confirms descending render order in all three

**Done** — `KanbanBoard` sorts each column's tasks by `weightedScore` descending. `ListView`'s sort replaced its old priority-enum-based primary key (`PRIORITY_ORDER`, removed) with `weightedScore` descending, keeping due-date as a tiebreaker. `TodoList` (which had no sorting at all before) now sorts the same way. Verified live: seeded tasks with priorities in scrambled creation order across all three surfaces — every one rendered correctly descending (Critical 9 → High 8 → Medium 7 → Low 4), confirming the sort is by computed score, not creation order or priority enum.

---

### PRI03.2 — Project selector default sort by criticality score

**File:** `frontend/packages/web/src/screens/ProjectSelectorScreen/`

**Acceptance criteria:**
- Projects render sorted by `CriticalityScore` descending by default
- Default ordering only
- Verified in-browser: seeding projects with distinct `CriticalityScore` values confirms descending render order

**Done** — `ProjectSelectorScreen` sorts projects by `criticalityScore` descending. Verified live with three test projects at distinct scores (9/5/2) — render order matched exactly, with same-score real projects preserving their original relative order (JavaScript's guaranteed-stable sort). This verification pass is also where the PRI02.5 "1234" incident (noted above) was caught and immediately fixed.

---

## PREP Tickets — Production Readiness & Environment Config

**Epic:** Production readiness & environment config — ✅ Done (Sprint 6) — all six tickets done and signed off across two rounds: PREP.1/PREP.2 signed off 2026-08-29, PREP.3–PREP.6 signed off 2026-08-30. Closes out Sprint 6 in full (paired with BUGS, already done) — this also completes [ROADMAP.md](ROADMAP.md#phase-8-bug-fixes--deployment-prep) Phase 8.

> Verified against current source before drafting (2026-08-28) — all six items confirmed still open at that time despite everything shipped since this was first scoped: `Program.cs` still used `AllowAnyOrigin()`/`"AllowAll"`, no `/health` endpoint existed, and the legacy `frontend/src/` tree was still present on disk.

| Ticket | Title | Points | Status | Blocked By |
|---|---|---|---|---|
| PREP.1 | Lock CORS to a DB-backed, admin-editable allow-list | 5 | ✅ Done | — |
| PREP.2 | Add `/health` endpoint | 1 | ✅ Done | — |
| PREP.3 | Verify `VITE_API_URL` / production build wiring | 2 | ✅ Done | — |
| PREP.4 | Review connection string / secrets handling | 2 | ✅ Done | — |
| PREP.5 | Confirm Serilog console sink for container stdout | 1 | ✅ Done | — |
| PREP.6 | Delete legacy `frontend/src/` tree + orphaned config | 1 | ✅ Done | — |

---

### PREP.1 — Lock CORS to a DB-backed, admin-editable allow-list

> **Scope note:** rewritten 2026-08-29 to match what was actually built. The original ticket (below, for history) described a static `AllowedOrigins` array in `appsettings`/an env var, requiring a redeploy to add or remove an origin. During implementation planning, the user raised that a static file means every future origin (a new deployment's frontend URL, a future browser-based integration) needs a redeploy just to allow it — so the decision was made with the user to make the allow-list **DB-backed and admin-editable at runtime instead**, mirroring the existing `ApiToken` feature (AUTH01.3): a simple admin-managed, DB-backed collection with list/create/delete, with no redeploy required to change it. Also clarified during planning: CORS only ever governs *browser* callers — it never applies to server-to-server callers (webhooks, Home Assistant), which already authenticate via the existing `ApiToken` mechanism from AUTH01, so nothing extra was needed for them.
>
> **Done — signed off 2026-08-29.** Implementation was complete and verified live on 2026-08-28/29 (see acceptance criteria below); the user gave explicit sign-off on 2026-08-29 per this project's ticket-closure convention (see PRI02.5 for a precedent of a similar mid-flight scope annotation).

**File:**
- `backend/TaskManager.Domain/Entities/AllowedOrigin.cs` (new)
- `backend/TaskManager.Infrastructure` — `AllowedOriginRepository` + `IUnitOfWork` wiring (new)
- EF migration `AddAllowedOrigin` (new)
- `AllowedOriginCache` (`IAllowedOriginCache`/`AllowedOriginCache`, new) — thread-safe singleton backing a dynamic CORS policy (`policy.SetIsOriginAllowed(cache.IsAllowed)`)
- `Application` — `GetAllowedOriginsQuery`, `AddAllowedOriginCommand`, `RemoveAllowedOriginCommand` + FluentValidation (new, mirrors the `ApiTokens` CQRS folder)
- `AllowedOriginsController` (new) — `[Authorize(Roles = "Admin")]`, `GET/POST /api/allowed-origins`, `DELETE /api/allowed-origins/{id}`
- `backend/TaskManager.API/Program.cs` — old `"AllowAll"` policy (`AllowAnyOrigin()`) removed, replaced with the dynamic policy above
- `DataSeeder.SeedDevelopmentDataAsync` — dev-only seed of `http://localhost:5173` (ordinary seed data, not a special-cased bypass)
- `@taskmanager/shared`'s `ApiClient.ts` — `AllowedOriginDto` + `getAllowedOrigins()`/`addAllowedOrigin()`/`removeAllowedOrigin()`
- `frontend/packages/web/src/hooks/useAllowedOrigins.ts` (new)
- `frontend/packages/web/src/screens/admin/AllowedOriginsScreen/` (new) — wired into `AdminShell.tsx`'s nav as "Allowed Origins"

**Points:** 5 *(was 2 — increased to reflect the actual scope: new entity + migration + repository + dynamic in-memory cache + CQRS + controller + admin screen, not a static-config change)*

**Goal:** Replace the wide-open `AllowAnyOrigin()` policy with a DB-backed, admin-editable allow-list of browser origins that takes effect immediately (no API restart/redeploy) when an admin adds or removes an origin.

**Acceptance criteria:**
- `AllowedOrigin` domain entity stores a normalized absolute http/https URL, validated in its constructor
- `AllowedOriginsController` (admin-only) supports listing, adding, and removing allowed origins via `GET/POST /api/allowed-origins` and `DELETE /api/allowed-origins/{id}`
- Adding or removing an origin takes effect on the live CORS policy immediately, with no API process restart, via `IAllowedOriginCache` being refreshed synchronously by the add/remove command handlers
- No `AllowAnyOrigin()` remains anywhere in the codebase (confirmed via repo-wide search)
- Dev environment works out of the box against the Vite dev server origin via a dev-only seeded row (`http://localhost:5173`), not a special-cased code bypass — an admin can delete/change this row like any other
- CORS enforcement applies only to browser callers; server-to-server callers (webhooks, Home Assistant) are unaffected, since they already authenticate via `ApiToken` (AUTH01) rather than going through a browser's CORS preflight
- Admin UI (`AllowedOriginsScreen`) lists current origins with add/remove controls, reachable from `AdminShell`'s nav
- Verified: a CORS preflight from a non-allowed origin does not receive `Access-Control-Allow-Origin`; a preflight from an allowed origin does; adding an origin via the admin UI/API makes a subsequent preflight from that origin succeed without restarting the API, and removing it makes the next preflight fail again — all observed live, not just at startup
- Verified in-browser: the real Vite dev app loads with no CORS console errors, and the `AllowedOriginsScreen` add/remove flow updates the table live against an authenticated admin session

<details>
<summary>Original ticket text (superseded, kept for history)</summary>

**File:** `backend/TaskManager.API/Program.cs`

**Points:** 2

**Goal:** Replace the wide-open `AllowAnyOrigin()` policy with an explicit, configurable allow-list before deployment.

**Acceptance criteria:**
- CORS policy replaced with `.WithOrigins(...)` sourced from configuration (e.g. an `AllowedOrigins` array in `appsettings`/an env var), not hardcoded in `Program.cs`
- No `AllowAnyOrigin()` remains anywhere in the codebase
- Dev environment still works against the existing Vite dev server origin via the same configuration mechanism — not a special-cased dev-only bypass
- Verified: a request from a non-whitelisted origin is rejected by the browser (CORS preflight fails); a request from the whitelisted origin succeeds unchanged

</details>

---

### PREP.2 — Add /health endpoint

> **Implementation note:** built and verified 2026-08-29 exactly to the acceptance criteria below — no scope changes, unlike PREP.1. Used the official `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` package's `AddDbContextCheck<AppDbContext>()` (reuses the app's existing `AppDbContext` and its real connection-string resolution logic, rather than duplicating it via a separate Npgsql-specific health-check package), plus `app.MapHealthChecks("/health").AllowAnonymous()` — the explicit `.AllowAnonymous()` was required because the app has a global `FallbackPolicy` requiring authentication on every endpoint by default; without it `/health` would 401 instead of reporting real health. Exactly two files changed: `TaskManager.API.csproj` and `Program.cs`.
>
> **Done — signed off 2026-08-29.** Built and verified exactly to the acceptance criteria below, no scope changes. The user gave explicit sign-off on 2026-08-29 per this project's ticket-closure convention (see PRI02.5 and PREP.1 for precedent).

**File:** `backend/TaskManager.API/Program.cs`, `backend/TaskManager.API/TaskManager.API.csproj`

**Points:** 1

**Goal:** Give Docker (D03) something to health-check against — nothing like this exists today.

**Acceptance criteria:**
- `GET /health` returns `200` when the API and its database connection are both up (ASP.NET Core's built-in health-check middleware, e.g. `AddHealthChecks().AddNpgSql(...)`)
- Returns a non-`200` status when the database connection is down
- Endpoint requires no authentication — Docker's healthcheck can't carry a JWT
- Verified: hitting `/health` locally returns `200` while Postgres is up; stopping the database and re-hitting it returns a failure status

**Verification performed (2026-08-29, all passed):**
- `dotnet build` clean
- `curl -i http://localhost:5062/health` with Postgres up → `200 OK`, body "Healthy", no `Authorization` header sent (confirms anonymous access)
- Stopped the project's Postgres container → re-curled `/health` → `503 Service Unavailable`, body "Unhealthy"
- Restarted the container → `/health` recovered to `200 OK`; a normal authenticated endpoint (`/api/projects`) still correctly required auth (401, not a crash)
- Confirmed via repo-wide search exactly one `MapHealthChecks` registration exists

---

### PREP.3 — Verify VITE_API_URL / production build wiring

> **Implementation note:** built and verified 2026-08-29. Baseline check found no `.env.production` and no bare `.env` in `frontend/packages/web/` — only `.env.development`/`.env.example` — so a real production build today silently falls back to `ApiClient.ts`'s hardcoded `http://localhost:5000` default (confirmed by running the build and grepping the output bundle). **Scope deviation from the ticket's literal "File: `.env.production` (new)" line:** no `.env.production` file was created. The real production API URL is genuinely unknown yet — this is a self-hosted deployment and depends on Phase 9's D03 Docker Compose network topology, which doesn't exist yet — so committing a placeholder URL into a tracked file would be misleading and would bit-rot. Vite's `loadEnv` precedence already solves this correctly on its own: a real build-time environment variable (`VITE_API_URL=https://... npm run build`) always overrides anything in a `.env.*` file. Instead, the mechanism was documented thoroughly in `frontend/packages/web/.env.example` (new "Production builds (PREP.3)" comment block: Vite bakes the var in statically at build time, not read at container runtime; no committed `.env.production` because the real URL is deployment-specific; the exact env-var-at-build-time syntax; and a note for Phase 9's D02 that Docker needs to pass it as a build `ARG`, not a runtime `ENV`). Only file changed: `frontend/packages/web/.env.example` — no backend changes, no new files.
>
> **Done — signed off 2026-08-30.** Built and verified exactly per the acceptance criteria below (with the documented `.env.production` deviation noted above). The user gave explicit sign-off on 2026-08-30 per this project's ticket-closure convention (see PREP.1/PREP.2 precedent).

**File:** `frontend/packages/web/.env.example` (updated — no `.env.production` created, see implementation note above), Vite config

**Points:** 2

**Goal:** Confirm the frontend can be pointed at a real production API URL at build time, not just the hardcoded dev value from `.env.development` (BUG-04).

**Acceptance criteria:**
- Production env mechanism documented (`.env.production` or equivalent) showing how `VITE_API_URL` gets set for a production build
- Confirms Vite bakes `VITE_API_URL` in at build time (standard Vite behavior) — documented so D02's Dockerfile build step knows to pass it as a build arg, not a runtime env var
- `.env.example` updated if the production guidance isn't already covered there
- Verified: a local production build (`npm run build --workspace=packages/web`) against a different `VITE_API_URL` value produces a bundle that calls the new URL

**Verification performed (2026-08-29, all passed):**
- Baseline: `npm run build --workspace=packages/web` with no override → grepped the built `dist/assets/*.js` → confirmed it silently baked in the `localhost:5000` fallback (reproduced the gap the ticket is about)
- Override: `VITE_API_URL=https://prod-test.example.com npm run build --workspace=packages/web` → grepped the new build output → confirmed `prod-test.example.com` was baked into the bundle and `localhost:5000`/`localhost:5062` did not appear — proves the build-time override mechanism works exactly as documented
- Follow-up plain build (no env var) → confirmed it reverted cleanly to the `localhost:5000` fallback, proving no lingering shell-state contamination between builds

---

### PREP.4 — Review connection string / secrets handling

> **Implementation note:** built and verified 2026-08-30. Confirmed `Program.cs` uses standard, unmodified ASP.NET Core configuration precedence (`appsettings.json` → `appsettings.{Environment}.json` → environment variables → command-line args) — nothing short-circuits it, so `DatabaseSettings__ConnectionString` as an env var already overrode file-based config with zero code changes needed. Confirmed no real/sensitive connection string or JWT secret is committed anywhere in the tracked production `appsettings.json` (no `DatabaseSettings`/`ConnectionStrings` section at all, `Jwt:Secret` is an empty string); `appsettings.Development.json`'s connection string has an obvious placeholder password, dev-scoped only. Live-proved the override: ran the API with `DatabaseSettings__ConnectionString` pointed at a different, nonexistent database name — EF Core's migration-on-startup auto-created and seeded it fresh (new system user, new `AllowedOrigins` row), unambiguously distinct from the real dev DB's existing state, confirming the env var won. The throwaway `doesnotexist` database was dropped afterward via `docker exec ... psql -c "DROP DATABASE..."` to avoid leaving test pollution on the shared Postgres container.
>
> **Scope addition beyond the ticket's original acceptance criteria** (flagged and approved by the user during planning — asked directly whether the extra hardening was worth it given the deployment plan is a single bundled Docker Compose stack (API+DB together); user said "add it anyway" as cheap insurance): added a small `IsProduction()`-gated startup guard in `Program.cs`. The existing DB connection-string resolution logic (previously inline in the `AddDbContext` lambda) was extracted into a local function `ResolveConnectionString(IServiceProvider)`, and the hardcoded dev-fallback connection string was hoisted into a `const string DevFallbackConnectionString` — pure refactor, no behavior change, just removes duplication so the guard can reuse the same resolution logic. Right after `var app = builder.Build();`: if `app.Environment.IsProduction()`, throws a clear `InvalidOperationException` if the resolved connection string still equals the dev fallback (naming `DatabaseSettings__ConnectionString` as the fix), and a second clear exception if `jwtSettings.Secret` is empty (naming `Jwt__Secret`). Both checks are no-ops outside `Production` — Development is completely unaffected (verified).
>
> **Note for future re-verification:** `dotnet run`'s default launch profile (`launchSettings.json`) hardcodes `ASPNETCORE_ENVIRONMENT=Development` and overrides a shell-level env var — verifying the Production paths required the `--no-launch-profile` flag to actually reach `Production` mode. Anyone re-checking this later needs that flag, or it'll look like the guard doesn't work when it's actually just `launchSettings.json` getting in the way.
>
> **Done — signed off 2026-08-30.** Built and verified per the acceptance criteria below, plus the scope-addition guard described above. The user gave explicit sign-off on 2026-08-30 per this project's ticket-closure convention (see PREP.1/PREP.2/PREP.3 precedent).

**File:** `backend/TaskManager.API/Program.cs` (only file changed)

**Points:** 2

**Goal:** Confirm the database connection string can be supplied via Docker secrets/env vars in production, not just `appsettings.Development.json`.

**Acceptance criteria:**
- Connection string resolution order confirmed: an environment variable override takes precedence over `appsettings.json` (standard ASP.NET Core configuration behavior) — verified it isn't being short-circuited anywhere in `Program.cs`'s existing `DatabaseSettings` binding
- No real/sensitive connection string is committed to any `appsettings*.json` in the repo
- Verified: overriding the connection string via an environment variable actually takes effect over the `appsettings.json` value

**Verification performed (2026-08-30, all passed):**
- `dotnet build` clean
- Confirmed `Development` still starts normally post-refactor (guard correctly skipped, DB connects fine, `/health` returns 200)
- Env-var override proof: `DatabaseSettings__ConnectionString` pointed at a nonexistent database name → EF Core auto-created and seeded it fresh with data distinct from the real dev DB, confirming the env var took precedence; throwaway database dropped afterward
- Production + nothing configured → app fails fast at startup with `InvalidOperationException: No database connection string configured for Production. Set the DatabaseSettings__ConnectionString environment variable.` (confirmed via actual crash output, not just code review)
- Production + valid DB connection string but no JWT secret → fails fast with `InvalidOperationException: No JWT signing secret configured for Production. Set the Jwt__Secret environment variable.`
- Production + both set correctly → started cleanly with no exception, ran for 12+ seconds with the background worker ticking normally, confirming it's genuinely alive and not silently stuck
- Confirmed via grep that production `appsettings.json` has no `DatabaseSettings`/`ConnectionStrings` section and `Jwt:Secret` is empty

---

### PREP.5 — Confirm Serilog console sink for container stdout

> **Implementation note:** built and verified 2026-08-30. Confirmed the Serilog Console sink is configured and active per the ticket's original acceptance criteria — `appsettings.json`'s `Serilog:WriteTo` includes a `Console` entry with a reasonable output template, confirmed live by running the API and observing real Console log output.
>
> **Scope addition beyond the ticket's original acceptance criteria** (flagged and approved by the user during planning): the user asked for friendly, memorable environment variables to repoint the File and Seq sinks in a deployment (e.g. `SEQ_URL`), instead of relying on the clunky, fragile nested `Serilog__WriteTo__N__Args__X` syntax (fragile because it depends on the sink's position in the `WriteTo` array — reordering the array would silently break it). Given a choice between friendly env vars that repoint sinks, friendly env vars that also toggle sinks on/off, or documenting the existing mechanism with no code change, the user chose friendly named env vars with both sinks staying always-registered by default (no on/off toggle).
>
> `appsettings.json`'s `Serilog:WriteTo` now only contains the `Console` entry (unchanged args); the File and Seq settings were moved out of the `WriteTo` array into their own simple top-level sections with the exact same default values as before (`"Seq": { "Url": "http://localhost:5341" }`, `"FileLogging": { "Path": "logs/taskmanager-.log" }`) — a pure relocation, not a behavior change. `Program.cs`'s `UseSerilog` callback now reads `SEQ_URL` (falling back to `Seq:Url`) and calls `configuration.WriteTo.Seq(seqUrl)` when non-empty, and reads `LOG_FILE_PATH` (falling back to `FileLogging:Path`) and calls `configuration.WriteTo.File(logFilePath, ...)` with the same rolling/size/output-template settings the File sink always had, when non-empty. Both sinks remain always-registered by default via the appsettings.json fallback values — behavior is unchanged unless the env var is actually set. `appsettings.Development.json` needed no changes (it never overrode File/Seq settings).
>
> **Done — signed off 2026-08-30.** Built and verified per the acceptance criteria below, plus the scope-addition env vars described above. The user gave explicit sign-off on 2026-08-30 per this project's ticket-closure convention (see PREP.1/PREP.2/PREP.3/PREP.4 precedent).

**File:** `backend/TaskManager.API/appsettings.json`, `backend/TaskManager.API/Program.cs`

**Points:** 1

**Goal:** A verification-only ticket — confirm logs will be visible via `docker logs` once containerized, not new work.

**Acceptance criteria:**
- Serilog console sink confirmed configured and active (not file-only)
- Log format is reasonable for container log aggregation (structured or plain, implementer's call — just not empty/missing)
- Verified: running the API locally shows Serilog output in the console/terminal

**Verification performed (2026-08-30, all passed):**
- `dotnet build` clean
- Default run (no env vars): Console output confirmed working; the default File sink log (`logs/taskmanager-20260830.log`) confirmed still receiving fresh writes at its original path, proving the appsettings.json relocation didn't change default behavior
- Seq override proof: ran with `SEQ_URL=not-a-valid-url` (deliberately malformed) → the app crashed at startup with `System.UriFormatException: Invalid URI...` thrown from inside `Serilog.Sinks.Seq.Http.SeqIngestionApiClient`'s constructor — unambiguous proof the env var value is actually read and passed straight into the Seq sink's construction, not silently ignored
- File override proof: ran with `LOG_FILE_PATH` pointed at a throwaway scratch path → confirmed a real log file appeared there with genuine log content, and confirmed via file mtime that the default `logs/taskmanager-*.log` file received no new writes during that run (its last-modified timestamp predated the override run) — proves the redirect was complete, not additive. The throwaway log file was deleted afterward.

---

### PREP.6 — Delete legacy frontend/src/ tree + orphaned config

> **Implementation note:** built and verified 2026-08-30. Deleted the entire legacy `frontend/src/` tree — a leftover from before the ARCH01 npm-workspaces monorepo split, when the whole app lived at the `frontend/` root before `packages/web`/`packages/mobile`/`packages/shared`/`packages/ui` existed — plus its orphaned root-level config, exactly the file list the ticket named: `frontend/index.html`, `frontend/vite.config.ts`, `frontend/tsconfig.json`, `frontend/tsconfig.app.json`, `frontend/tsconfig.node.json`, `frontend/tailwind.config.js`, `frontend/postcss.config.js`, `frontend/capacitor.config.ts`.
>
> **Scope addition beyond the ticket's literal file list** (presented directly in the plan as "found beyond the ticket's literal list, proposing to include" and approved as part of the overall plan approval — not a case requiring separate user clarification like some earlier PREP additions): also deleted three more root-level items confirmed dead during investigation — `frontend/public/` (favicon.svg/icons.svg/sql-wasm.wasm, only ever served the now-deleted root `index.html`), `frontend/dist/` (stale build output of the now-deleted root `vite.config.ts`), and `frontend/eslint.config.js` (not referenced by any `lint` script anywhere in the repo — genuinely orphaned, not just relocated).
>
> **Pre-existing, unrelated gap noted but explicitly NOT fixed here:** `packages/web/index.html` links `href="/favicon.svg"` but `packages/web` has no `public/` directory of its own — a 404 that predates this deletion and is unaffected by it (confirmed the built `dist/` had no favicon even before this ticket). Flagged as a loose end for a future ticket, not part of PREP.6's scope.
>
> Files explicitly confirmed **not** touched (legitimate active parts of the monorepo): `frontend/.gitignore`, `frontend/README.md`, `frontend/package.json`, `frontend/package-lock.json`, `frontend/node_modules/`, `frontend/packages/`.
>
> **Done — signed off 2026-08-30.** Built and verified per the acceptance criteria below, plus the scope-addition deletions described above. The user gave explicit sign-off on 2026-08-30 per this project's ticket-closure convention (see PREP.1/PREP.2/PREP.3/PREP.4/PREP.5 precedent). This was the last of the six PREP tickets to reach "implemented, awaiting sign-off" — with all six signed off together on 2026-08-30, the epic closes in full.

**File:** `frontend/src/` (delete), `frontend/vite.config.ts`, `index.html`, `tsconfig*.json`, `tailwind.config.js`, `postcss.config.js`, `capacitor.config.ts` (delete if present at repo root, outside `packages/*`) — also `frontend/public/`, `frontend/dist/`, `frontend/eslint.config.js` (scope addition, see implementation note above)

**Points:** 1

**Goal:** Remove dead code confirmed unreferenced by the npm workspace since the ARCH01 split.

**Acceptance criteria:**
- `frontend/src/` directory deleted entirely
- Orphaned root-level config files listed above deleted
- `npm install` and `npm run build --workspace=packages/web` both still succeed after deletion, confirming nothing was actually referencing the deleted tree
- Verified: a repo-wide search for any import path referencing `frontend/src/` returns zero results post-deletion

**Verification performed (2026-08-30, all passed):**
- Repo-wide grep across `frontend/packages/**` before deletion confirmed nothing referenced `frontend/src` — zero results
- Confirmed `packages/web` has its own fully self-contained equivalents of every deleted root config file, with no `extends`/path references back to the root versions
- Confirmed `packages/mobile` (the Capacitor consumer) has its own independent dependency set and no reference to the root `capacitor.config.ts`
- `npm install` from `frontend/` succeeded cleanly post-deletion (lockfile still resolves)
- `npm run build --workspace=packages/web` succeeded, produced `dist/` normally
- Repo-wide grep for `frontend/src` post-deletion → only matches in planning docs (TASKS.md/TICKETS.md/ROADMAP.md/EPIC-ENTITY-PLAN.md — ticket descriptions, not code/import paths) — zero actual code references
- Live browser check: dev server renders correctly (login screen, correct "TaskManager" branding, not the old legacy app's generic title), no console errors
- `frontend/` root directory listing post-deletion confirmed to contain only the expected surviving files: `.gitignore`, `README.md`, `package.json`, `package-lock.json`, `node_modules/`, `packages/`

---

## D01–D03 Tickets — Monolithic Production Docker Image

**Epic:** Sprint 7 — Initial Deployment (Monolithic Production Image) — ✅ Done — all six tickets (D01.1–D01.2, D02.1–D02.2, D03.1–D03.2) signed off 2026-08-31. Closes out Sprint 7 in full — this also completes [ROADMAP.md](ROADMAP.md#phase-9-containerization--initial-deployment) Phase 9.

> Redefined per user direction (2026-08-30): rather than three separate images orchestrated via docker-compose (API, frontend, Postgres), production is a single monolithic container — Postgres, the API, and the built frontend all run together under one process supervisor (s6-overlay), for simpler operation on a single-host Unraid deployment. D01 covers the Dockerfile build stages, D02 covers the shared runtime base all three processes live in, and D03 wires the single image's internal routing/startup order and verifies the whole thing end-to-end. MCP server integration (M04) still comes later, as its own separate container, per [ROADMAP.md](ROADMAP.md#phase-11-post-deployment--mcp-server-agent-integration-layer).

### D01 Tickets — Backend & Frontend Build Stages

**Epic:** Backend & frontend build stages — ✅ Done (Sprint 7)

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| D01.1 | .NET API publish build stage | ✅ Done | — |
| D01.2 | Frontend Vite build stage | ✅ Done | — |

---

### D01.1 — .NET API Publish Build Stage

**File:** `Dockerfile` (repo root — final location decided during implementation)

**Goal:** A Dockerfile build stage that produces a clean, published Release build of `TaskManager.API`, ready to be copied into the final runtime image.

**Acceptance criteria:**
- A named build stage (e.g. `build-api`) using the .NET 10 SDK image restores and `dotnet publish`es `TaskManager.API` in Release configuration
- `docker build --target build-api .` succeeds in isolation and produces a publish output directory
- No SDK, source code, or build-only files are present past this stage — only the publish output is available for later stages to copy from

> **Done — signed off 2026-08-31.** Built as the `build-api` stage in a single root-level `Dockerfile`, using the .NET 10 SDK image to `dotnet publish` `TaskManager.API` in Release. The pre-existing standalone `backend/Dockerfile` (found during exploration, API-only) was deleted — superseded by this stage since it predated the monolithic-container decision. Verified: `docker build --target build-api .` succeeds in isolation.

---

### D01.2 — Frontend Vite Build Stage

**File:** Same `Dockerfile`, second named build stage

**Goal:** A Dockerfile build stage that produces the static production build of `packages/web`, configured to call the API via a relative path so no external domain needs to be known at build time.

**Acceptance criteria:**
- A named build stage (e.g. `build-web`) using a Node image runs `npm ci` then `npm run build --workspace=packages/web`
- `VITE_API_URL` is set to a relative path (e.g. `/api`) for the production build, since Nginx will proxy that path to the local API process in the same container (see D03.1) — no absolute production domain needs to be baked in
- `docker build --target build-web .` succeeds in isolation and produces a `dist/` output directory
- Manual check confirms the built bundle's JS references `/api/...` and not `localhost:5062` or any other dev-only URL

> **Done — signed off 2026-08-31.** Built as the `build-web` stage in the same root `Dockerfile`, running `npm ci` + `npm run build --workspace=packages/web`. **Deviation from the acceptance criteria as literally written:** `VITE_API_URL` is set to an **empty string**, not `/api` — `frontend/packages/shared/src/ApiClient.ts`'s `BASE_URL` is used as axios's `baseURL` *and* is prepended directly to endpoint paths that already embed `/api/...` themselves (and to `/avatars/...` asset paths), so `/api` would have doubled to `/api/api/projects`. An empty string resolves everything relative to the page's own origin correctly. Verified: `docker build --target build-web .` succeeds in isolation.

---

### D02 Tickets — Monolithic Runtime Base (Postgres + Nginx + Supervisor)

**Epic:** Monolithic runtime base (Postgres + Nginx + supervisor) — ✅ Done (Sprint 7)

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| D02.1 | Runtime base image: Postgres + .NET runtime + Nginx + process supervisor | ✅ Done | — |
| D02.2 | Postgres data volume + non-root permissions | ✅ Done | D02.1 |

---

### D02.1 — Runtime Base Image: Postgres + .NET Runtime + Nginx + Process Supervisor

**Goal:** A final-stage Alpine-based image with everything needed to run Postgres, the .NET API, and Nginx as separate supervised processes in one container.

**Acceptance criteria:**
- Final stage installs Postgres server, the .NET 10 ASP.NET runtime (not the SDK), and Nginx, on an Alpine base
- A process supervisor (s6-overlay) is installed and configured with one service definition per process: `postgres`, `api`, `nginx`
- `docker build` for the final stage succeeds and produces a runnable image (the three binaries are present and s6 is configured, even before D02.2/D03's data/wiring work lands)

> **Done — signed off 2026-08-31.** Final stage of the root `Dockerfile`: Postgres 16 + .NET 10 ASP.NET runtime + Nginx on Alpine, supervised by s6-overlay v3, non-root `appuser`. Service definitions under `docker/s6-overlay/s6-rc.d/{postgres-init,postgres,api,nginx}/` (one oneshot init + three longruns), each a thin `execlineb` wrapper running `with-contenv` before handing off to the real logic in `docker/s6-overlay/scripts/`. **Notable finding:** `with-contenv` is required because s6-overlay v3 does **not** auto-inherit Docker's env vars into supervised services — a non-obvious behavior that would otherwise leave `Jwt__Secret`/connection strings invisible to the API process. All four services registered in `docker/s6-overlay/s6-rc.d/user/contents.d/`. Verified: `docker build -t taskmanager:local .` succeeds and all three binaries/s6 config are present in the image.

---

### D02.2 — Postgres Data Volume + Non-Root Permissions

**Goal:** Postgres data persists across container recreation, and the whole container runs as a non-root user.

**Acceptance criteria:**
- Postgres's data directory is a declared `VOLUME`, pointed at a path intended to be bind-mounted from the host (e.g. `/mnt/user/appdata/taskmanager/pgdata` on Unraid)
- The container runs as a non-root user; that user owns the Postgres data directory and any other writable paths (e.g. the avatar upload directory from A02, `wwwroot/avatars`)
- A fresh `docker run` against an empty/new volume auto-initializes the Postgres data directory with no manual `initdb` step required
- A second `docker run` against the same already-initialized volume starts Postgres normally without re-initializing or erroring

> **Done — signed off 2026-08-31.** `docker/s6-overlay/scripts/postgres-init.sh` runs `initdb`/`createdb` on first boot only — idempotent, skips if the data directory already has content. Postgres/API/Nginx run scripts each drop from root to `appuser` via `s6-setuidgid` before exec'ing. **Real fix required:** the `postgresql16` Alpine package creates its own system `postgres` user and locks `/var/lib/postgresql` to `drwxr-x---`, which blocked `appuser` from even traversing into it — required an explicit `chown -R appuser:appuser /var/lib/postgresql` in the Dockerfile (not just the `$PGDATA` subdirectory). Postgres's Unix socket directory is also redirected to `/tmp` rather than the default `/run/postgresql`, avoiding the need to create/chown a tmpfs directory. Verified: fresh volume auto-initializes with no manual step; a second run against the same volume starts normally and `postgres-init` correctly detects and skips re-initialization.

---

### D03 Tickets — Assemble, Wire & Verify the Production Image

**Epic:** Assemble, wire & verify the production image — ✅ Done (Sprint 7)

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| D03.1 | Wire Nginx, expose the API directly, startup order, and secrets | ✅ Done | D01.1, D01.2, D02.1 |
| D03.2 | Health check, persistent volumes, end-to-end verification | ✅ Done | D03.1, D02.2 |

---

### D03.1 — Wire Nginx, Expose the API Directly, Startup Order, and Secrets

**Goal:** All three processes talk to each other correctly inside the container, the API is reachable both through the frontend's proxy and directly on its own port, and Production secrets are supplied safely.

**Acceptance criteria:**
- Nginx serves the built frontend bundle (from D01.2) on its exposed port and reverse-proxies `/api/*` requests to the locally-running API process, for the frontend's own use
- The API's own port (e.g. `8080`) is separately declared with its own `EXPOSE` and documented for `docker run -p` publishing, so it is directly reachable on its own port too — independent of the Nginx proxy — for future consumers like the MCP server (Phase 11) or webhook integrations (B05b)
- `DatabaseSettings__ConnectionString` defaults to the local Postgres instance (`localhost`, same container) with no configuration required out of the box, while staying overridable via env var
- `Jwt__Secret` (and any other Production-required secret per PREP.4's startup guard) must be supplied via `docker run -e`; the container fails fast with PREP.4's existing clear error if it's missing, rather than starting silently misconfigured
- s6-overlay startup order: Postgres is confirmed healthy before the API starts; the API starts before/independently of Nginx (Nginx can still serve the static frontend even if the API isn't up yet — only proxied `/api` calls fail until it is)

> **Done — signed off 2026-08-31.** `docker/nginx.conf` serves the frontend on port 8081 and proxies both `/api/*` **and** `/avatars/*` to the API on `localhost:8080` — avatars needed a second proxy rule since they're served outside the `/api` prefix by the backend's static file middleware. The bundled Postgres uses a distinct internal connection string/password rather than PREP.4's literal dev-fallback string, so PREP.4's Production startup guard doesn't misfire even though `localhost` genuinely is correct here (Postgres is in the same container). **A real, non-obvious bug was found and fixed during verification:** `docker/s6-overlay/scripts/api-run.sh` originally didn't `cd` into `/app/api` before exec'ing `dotnet TaskManager.API.dll` by absolute path — ASP.NET Core's `ContentRootPath` comes from the process's working directory, not the assembly's location, so `appsettings.json` silently failed to load, `Jwt:Issuer`/`Jwt:Audience` were never bound, minted JWTs silently omitted their `iss`/`aud` claims, and every authenticated request failed with 401 forever, even immediately after a successful login. Fixed by adding `cd /app/api` before the exec. Verified: s6 brings up Postgres → API → Nginx in the correct order; omitting `Jwt__Secret` fails fast with PREP.4's existing error rather than starting silently misconfigured.

---

### D03.2 — Health Check, Persistent Volumes, End-to-End Verification

**Goal:** The image is production-ready: monitorable, keeps its data across restarts, and has been proven to work end-to-end.

**Acceptance criteria:**
- A Dockerfile `HEALTHCHECK` instruction hits PREP.2's `/health` endpoint through the container's exposed port
- Volumes are declared and documented for both Postgres data and avatar uploads (A02), both intended to live under `/mnt/user/appdata/taskmanager/` on the host
- Fresh verification: `docker run` with empty/new volumes and required env vars set → the app is reachable in a browser, the AUTH02 first-run setup flow completes, and a project can be created end-to-end
- Persistence verification: stop and remove the container, `docker run` again against the *same* volumes → all previously-created data (users, projects, tasks) is still present
- Direct API verification: confirm the API responds correctly when hit on its own exposed port (D03.1), independent of the Nginx-served frontend

> **Done — signed off 2026-08-31.** Pre-existing `docker-compose.yml` (previously two separate `backend`+`db` services, no frontend, no `Jwt__Secret`, Postgres's `5432` exposed straight to the host) rewritten to a single `app` service around the new monolithic image — `JWT_SECRET` required via a `.env` file (`.env.example` added documenting it), Postgres no longer exposed externally, volumes for `/mnt/user/appdata/taskmanager/pgdata` and `/mnt/user/appdata/taskmanager/avatars`. Full verification performed live (not simulated): `docker build` succeeds; fresh empty-volume boot brings the app up healthy on `/health` with the frontend loading on 8081; a full browser walkthrough completed the AUTH02 first-run admin setup, logged in, and created a project end-to-end through the real UI; the API was confirmed independently reachable on its own port 8080 (both an unauthenticated and an authenticated request matched behavior on 8081); stopping/removing the container and re-running against the same volumes preserved the same login and project, with `postgres-init` correctly skipping re-initialization; omitting `Jwt__Secret` on a fresh run correctly failed fast with PREP.4's existing error instead of starting silently misconfigured.

---

## MCP01 Tickets — gRPC Contracts + API gRPC Surface

**Epic:** gRPC Contracts + API gRPC Surface — ✅ Done (Sprint 9, signed off 2026-09-17)

> Establishes the shared gRPC contract between `TaskManager.API` and the future `TaskManager.Mcp` (MCP03), plus the API-side gRPC services that mirror the existing REST controllers. See [MCP-ARCHITECTURE.md](MCP-ARCHITECTURE.md) for the full topology and [TASKS.md](TASKS.md#mcp01) for the epic summary. Alongside this epic, `TaskManager.API`'s `Program.cs` was also refactored (ad hoc, user-requested) into `backend/TaskManager.API/Startup/WebApplicationBuilderExtensions.cs`/`WebApplicationExtensions.cs` — not a ticket of its own, but touches the same file MCP01.2's Kestrel wiring lives in, so it's noted here for anyone reading this epic's history.

| Ticket | Title | Status | Blocked By |
|---|---|---|---|
| MCP01.1 | `TaskManager.Grpc.Contracts` project + proto definitions | ✅ Done | — |
| MCP01.2 | Second internal-only gRPC Kestrel endpoint + `AddGrpc()` wiring | ✅ Done | MCP01.1 |
| MCP01.3 | `ProjectsGrpcService` + `EpicsGrpcService` | ✅ Done | MCP01.2 |
| MCP01.4 | `TasksGrpcService` + `PhasesGrpcService` | ✅ Done | MCP01.2 |

---

### MCP01.1 — `TaskManager.Grpc.Contracts` Project + Proto Definitions

**File:** `backend/TaskManager.Grpc.Contracts/` (new project)

**Goal:** Establish the shared gRPC contract between `TaskManager.API` and `TaskManager.Mcp`.

**Acceptance criteria:**
- New `backend/TaskManager.Grpc.Contracts/` project added to the solution, referencing `Grpc.Tools`/`Google.Protobuf` for codegen
- `projects.proto`, `epics.proto`, `tasks.proto`, `phases.proto` define messages mirroring `ProjectDto`, `EpicDto`, `ProjectTaskDto`, `ProjectPhaseDto` field-for-field
- Service definitions mirror the REST surface 1:1: `ListProjects`, `GetProject`, `ListEpicsByProject`, `CreateEpic`, `UpdateEpic`, `ListPhasesByProject`, `ListTasksByProject`, `GetTask`, `CreateTask`, `UpdateTask`, `TransitionTask`
- `dotnet build backend/TaskManager.Grpc.Contracts` succeeds and produces generated C# client and server base classes for every service
- No other project references `TaskManager.Grpc.Contracts` yet — this ticket is contract-only (MCP01.2 and MCP03.3 consume it)

> **Done — signed off 2026-09-17.** New net10.0 classlib added to `TaskManager.slnx`, referencing `Grpc.Tools`+`Google.Protobuf`+`Grpc.Core.Api` — the ticket named only the first two, but generating **server** base classes via `GrpcServices="Both"` also needs `Grpc.Core.Api` at compile time, confirmed by building. Four proto files mirror the DTOs field-for-field: `phases.proto`/`epics.proto`/`tasks.proto` (leaves, no imports) and `projects.proto` (imports all three, since `ProjectDto` embeds Phases/Epics/Tasks). Guid fields → `string`; nullable scalars → proto3 `optional`; `Dictionary<Guid,int>`/`Dictionary<string,string>` → proto `map<string,...>`; enums prefixed (`TASK_STATUS_*`/`TASK_PRIORITY_*`/`PROJECT_SCOPE_*`) per proto style guide. Needed `ProtoRoot="Protos"` on the `<Protobuf>` MSBuild item — the default per-file proto root fails cross-file imports with "File not found" otherwise. Verified: full-solution `dotnet build` succeeds; both `*Base` (server) and `*Client` (client) classes generated for all 4 services; confirmed no other project references it yet.

---

### MCP01.2 — Second Internal-Only gRPC Kestrel Endpoint + `AddGrpc()` Wiring

**File:** `backend/TaskManager.API/Program.cs`, `backend/TaskManager.API/TaskManager.API.csproj`

**Goal:** Give `TaskManager.API` a gRPC-capable, internal-only network surface to host the new services on.

**Acceptance criteria:**
- `TaskManager.API.csproj` references `TaskManager.Grpc.Contracts` and `Grpc.AspNetCore`
- `Program.cs` calls `builder.Services.AddGrpc()`
- A second Kestrel endpoint is bound via `ListenAnyIP(<grpc-port>, o => o.Protocols = HttpProtocols.Http2)` — cleartext HTTP/2, distinct from the existing REST port
- The new gRPC port is not referenced anywhere in the Nginx config under `docker/` — confirmed not proxied, matching the existing plain-HTTP API port's direct/internal posture (per the D01–D03 README note on "API (direct, e.g. for the MCP server or webhooks)")
- `dotnet build` succeeds with no gRPC services registered yet — this ticket is transport-only; MCP01.3/MCP01.4 add the actual services

> **Done — signed off 2026-09-17.** `TaskManager.API` now references `TaskManager.Grpc.Contracts` + `Grpc.AspNetCore`; a second cleartext HTTP/2 (h2c) Kestrel endpoint binds on port 8082 alongside the existing REST endpoint (logic now lives in `Startup/WebApplicationBuilderExtensions.cs`'s `ConfigureGrpc`, after the later Program.cs refactor). **A real gotcha caught by testing empirically before touching the real app:** calling `ListenAnyIP` inside `ConfigureKestrel` silently disables `ASPNETCORE_URLS`-based endpoint binding entirely — confirmed with a throwaway console app (Kestrel logs "Overriding address(es)... Binding to endpoints defined via IConfiguration and/or UseKestrel() instead" and drops the REST endpoint otherwise). Fixed by parsing the existing `ASPNETCORE_URLS`/`urls` config and re-declaring those REST endpoint(s) (with `UseHttps()` if the scheme is https) alongside the new gRPC-only endpoint. Verified: full-solution build succeeds with no gRPC services mapped yet (transport-only, as scoped); `docker/nginx.conf` untouched, doesn't reference port 8082; later confirmed live (during MCP01.3/MCP01.4's verification and the Program.cs refactor) that both ports actually bind and REST keeps serving traffic correctly end-to-end against the real dev DB.

---

### MCP01.3 — `ProjectsGrpcService` + `EpicsGrpcService`

**File:** `backend/TaskManager.API/Grpc/ProjectsGrpcService.cs`, `backend/TaskManager.API/Grpc/EpicsGrpcService.cs`

**Goal:** Expose Projects and Epics over gRPC without duplicating business logic.

**Acceptance criteria:**
- `ProjectsGrpcService` implements the generated base class, calling the same MediatR queries `ProjectsController` already calls (`ListProjects`, `GetProject`) and mapping results to proto messages
- `EpicsGrpcService` implements `ListEpicsByProject`, `CreateEpic`, `UpdateEpic` the same way
- Both services are registered via `app.MapGrpcService<T>()` and carry `[Authorize]`, matching the REST controllers' auth posture
- A gRPC call with no token returns `UNAUTHENTICATED`; a gRPC call with a valid token returns data matching the equivalent REST response for the same underlying data (spot-checked live with a gRPC test client, e.g. `grpcurl` or a throwaway console client)
- `ProjectsController`/`EpicsController` are unmodified — the new services sit next to them, not instead of them

> **Done — signed off 2026-09-17.** New `ProjectsGrpcService`/`EpicsGrpcService`, both `[Authorize]`-gated and registered via `MapGrpcService<T>()`, plus a shared `GrpcMappingExtensions.cs` (DTO↔proto mapping, reused by MCP01.4). `ProjectsGrpcService` implements `ListProjects`/`GetProject`; `EpicsGrpcService` implements `ListEpicsByProject`/`CreateEpic`/`UpdateEpic` — all calling the exact same MediatR queries/commands the REST controllers already call. **Real bug caught before it ran:** `ProjectDto.Scope`/`ProjectTaskDto.Status`/`ProjectTaskDto.Priority` are plain `string` (via `.ToString()` in `MappingExtensions.cs`), not the domain enum types — mapping needed `System.Enum.Parse<T>` first (fully qualified, since `Google.Protobuf.WellKnownTypes.Enum` collides with `System.Enum` once that namespace is imported). Verified fully live against the real dev DB: minted a throwaway JWT locally using the known dev signing secret (no real password needed), ran the real API, drove a throwaway gRPC console client through the full matrix — no-token → `UNAUTHENTICATED`; `ListProjects`/`GetProject` with token → data matches REST exactly, field-by-field, cross-checked via `curl`; bad-GUID → `InvalidArgument`; nonexistent id → `NotFound`; `CreateEpic`/`UpdateEpic` against a dedicated throwaway project → both work, partial-update semantics correct (unset field ⇒ unchanged); cross-checked final state via REST afterward, identical. `ProjectsController`/`EpicsController` untouched.

---

### MCP01.4 — `TasksGrpcService` + `PhasesGrpcService`

**File:** `backend/TaskManager.API/Grpc/TasksGrpcService.cs`, `backend/TaskManager.API/Grpc/PhasesGrpcService.cs`

**Goal:** Expose Tasks and Phases over gRPC, completing the hierarchy surface.

**Acceptance criteria:**
- `TasksGrpcService` implements `ListTasksByProject`, `GetTask`, `CreateTask`, `UpdateTask`, `TransitionTask`, calling the same MediatR handlers `TasksController` already calls
- `PhasesGrpcService` implements `ListPhasesByProject` the same way
- Both carry `[Authorize]`; registered via `app.MapGrpcService<T>()`
- A gRPC `TransitionTask` call against a real dev-DB task produces the same status change and the same validation errors (e.g. an invalid transition) as the equivalent REST call
- `TasksController`/`PhasesController` are unmodified
- Full-solution build check: `dotnet build` succeeds across `TaskManager.Domain`, `TaskManager.Application`, `TaskManager.Infrastructure`, `TaskManager.API`, and `TaskManager.Grpc.Contracts` together, with every gRPC service having a matching client stub generated from the same `.proto` files — confirms a contract change can't be picked up on only one side without failing the build

> **Done — signed off 2026-09-17.** New `TasksGrpcService`/`PhasesGrpcService`, same `[Authorize]`+`MapGrpcService<T>()` pattern. `TasksGrpcService` implements `ListTasksByProject`/`GetTask`/`CreateTask`/`UpdateTask`/`TransitionTask`; `PhasesGrpcService` implements `ListPhasesByProject`. Added reverse `ToDomain()` enum mappers (proto→domain) to `GrpcMappingExtensions.cs` alongside MCP01.3's forward `ToGrpc()` ones. **Found and fixed a real gap affecting MCP01.3 too, not just this ticket's new code:** neither `EpicsGrpcService` nor the new Tasks service mapped `FluentValidation.ValidationException`/domain `InvalidOperationException` (e.g. `ProjectTask.Transition`'s invalid-state guard) to a clean gRPC status — both would have surfaced as a generic `Unknown` instead of matching REST's 400/422. Fixed with a new shared `backend/TaskManager.API/Grpc/GrpcExceptionMapping.cs` (`ValidationException`→`InvalidArgument`, `InvalidOperationException`→`FailedPrecondition`), applied to every mutating RPC in both `EpicsGrpcService` (retrofit) and `TasksGrpcService`. Verified live end-to-end (13 scenarios): no-token → `UNAUTHENTICATED` on every service; empty-title/empty-name → `InvalidArgument` with the FluentValidation message (both Tasks and the Epics retrofit); full task lifecycle via gRPC (`CreateTask`→`GetTask`→`UpdateTask`→`TransitionTask` Backlog→InProgress→Done) with the invalid transition Done→InProgress correctly returning `FailedPrecondition`, matching REST's `InvalidOperationException`→422 exactly; `ListPhasesByProject` works and is auth-gated; cross-checked final task state via REST afterward, identical. Full-solution `dotnet build` succeeds across all 5 backend projects together, satisfying the cross-project build-check criterion. `TasksController`/`PhasesController` untouched.

