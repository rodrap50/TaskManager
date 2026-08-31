# HomeAssistant Project Tracker — Mission & Architecture

## 🎯 North Star

To build a high-performance, completely self-hosted, and privacy-first project management system that replaces rigid commercial platforms — combining Trello's visual simplicity with Jira's robust tracking. The application must provide a seamless user experience for logging and tracking large/small projects, daily tasks, and multi-user assignments, while maintaining absolute data sovereignty.

---

## 🧭 Core Pillars

1. **Absolute Privacy & Open Extensibility**
   Zero telemetry, zero cloud dependencies. The system exposes a secure, containerized REST API and inbound webhook gateway for seamless integration with local tools: Home Assistant, MQTT brokers, custom dashboard widgets.

2. **Local-First Resiliency**
   The mobile app is fully functional offline with client-side SQLite caching and atomic background sync. The web app is always-online (direct API calls — no local DB).

3. **Engineering Excellence**
   Strict adherence to SOLID, KISS, and DRY. Traditional .NET Controllers only (no Minimal APIs). Decoupled, testable Clean Architecture layers. No abstractions beyond what the task requires.

4. **Frictionless Jira/Trello UX**
   Optimized for instant data entry. Clear prioritization (Low / Medium / High / Critical), multi-user assignments, macro-to-micro views (Epics → Projects → Daily Tasks).

---

## 🛠 Engineering Principles

| Principle | Application |
|---|---|
| **SOLID** | Single responsibility per class/handler, strict interface abstractions, decoupled layers |
| **KISS** | Straightforward implementations. No abstract factories where basic DI suffices |
| **DRY** | Shared utilities (validation, query filters, API client) without over-coupling |
| **No Minimal APIs** | All endpoints use traditional ASP.NET Core Controllers |
| **CQRS via MediatR** | Commands and queries as discrete handler classes in the Application layer |
| **Plan before code** | No implementation without an approved plan |

---

## 🏗 System Architecture

### Backend — .NET 10 Clean Architecture

```
HomeAssistantProjectPlanning/
  src/
    Domain/          ← Entities, value objects, enums (no framework dependencies)
    Application/     ← MediatR handlers, DTOs, validators, interfaces
    Infrastructure/  ← EF Core, repositories, UnitOfWork, bcrypt
    API/             ← ASP.NET Core controllers, middleware, Program.cs
```

**Key decisions:**
- PostgreSQL via Npgsql + EF Core (Fluent API, no data annotations)
- `AuditInfo` owned value object on every entity (`CreatedAt`, `UpdatedAt`, `RowVersion`)
- `ExternalMetadata` as `jsonb` column — stamps from external tools (Home Assistant, MCP agents, etc.)
- `DataSeeder` provides well-known system user `00000000-0000-0000-0000-000000000001` on first boot
- Auto-migration on startup

### Frontend — npm Workspaces Monorepo

```
frontend/
  package.json                  ← workspace root
  packages/
    shared/   (@taskmanager/shared)   ← axios API client + all TypeScript DTOs
    ui/       (@taskmanager/ui)       ← pure presentational React components
    web/      (@taskmanager/web)      ← always-online WebUI (MVP)
    mobile/   (@taskmanager/mobile)   ← Capacitor + offline-first SQLite (post-MVP)
```

| Package | Purpose | Status |
|---|---|---|
| `@taskmanager/shared` | Axios instance, all DTO interfaces, named API functions | ✅ Active |
| `@taskmanager/ui` | TaskCard, PriorityBadge, StatusIndicator, AssigneeAvatar | ✅ Active |
| `@taskmanager/web` | Always-online WebUI — direct API calls, no SQLite | ✅ Active (MVP) |
| `@taskmanager/mobile` | Capacitor + jeep-sqlite + mutation queue sync engine | ⏸ Preserved, deferred |

**Vite workspace aliases** in `packages/web/vite.config.ts` resolve `@taskmanager/shared` and `@taskmanager/ui` directly from `src/` — no build step needed during development.

**Dev server:** `npm run dev --workspace=packages/web` → `http://localhost:5173`

**File organization convention:** Applied incrementally as files are touched — not a retroactive sweep. Each component or screen gets its own subdirectory:

```
ComponentName/
  ComponentName.tsx        ← markup + behavior
  ComponentName.styles.ts  ← export const useComponentNameStyles = () => ({ ... })
  index.ts                 ← export { ComponentName } from './ComponentName';
```

Styles are always a hook (`useXStyles()`), never a plain exported constant — keeps the door open for prop/state-driven class logic without changing the calling convention. `index.ts` keeps import paths short (`from './ProjectCard'`, not `from './ProjectCard/ProjectCard'`). Applied so far to `App/`, `App/FloatingPill/`, `App/FloatingNav/`, and `screens/ProjectSelectorScreen/` (plus its `ProjectCard/` and `CreateProjectModal/` children).

### MCP Server (Phase 9)

A lightweight Node.js/TypeScript MCP (Model Context Protocol) server that exposes the task manager to AI agents (Claude Code, Claude Desktop). Runs as a separate Docker container on the internal network; communicates with the .NET API via REST.

### Deployment Target

Self-hosted on **Unraid** via Docker Compose. All persistent data in `/mnt/user/appdata/taskmanager/`. Services reachable over local network and Tailscale VPN. No external cloud dependencies.

---

## 🎨 Design & Style Decisions

Established during Sprint 1b (DS01–DS02) UI work. These are binding conventions for future screens, not one-off choices.

### Design tokens (Tailwind v4, CSS-first)

No `tailwind.config.ts` — Tailwind v4's CSS-first `@theme` block in `packages/web/src/index.css` is the single source of truth. `@theme` defines **dark-mode values by default**; a `.light` class on the root element overrides the mode-dependent tokens. `--color-primary-*` (crimson) is intentionally absent from `.light` — it stays constant across both modes.

| Token | Dark (default) | Light | Use |
|---|---|---|---|
| `bg-surface` | `#09090b` | `#f4f4f5` | Page ground |
| `bg-surface-raised` | `#18181b` | `#ffffff` | Cards, chrome, panels |
| `bg-surface-overlay` | `#27272a` | `#fafafa` | Modals, popovers |
| `border-border` | `rgb(63 63 70/0.5)` | `#d4d4d8` | Default borders |
| `border-border-subtle` | `#27272a` | `#e4e4e7` | Hover/lightened borders |
| `text-text` | `#fafafa` | `#09090b` | Primary text |
| `text-text-muted` | `#a1a1aa` | `#71717a` | Secondary text |
| `text-text-inverted` | `#ffffff` | `#ffffff` | Text on filled primary buttons |
| `bg-primary-900/800/700` | `#7f1d1d` / `#991b1b` / `#b91c1c` | *(same — mode-invariant)* | Deep crimson brand accent |

No raw Tailwind color utilities (`gray-*`, `blue-*`, `red-*`, `dark:` variants) in any screen that has been through a DS ticket. Screens not yet migrated (e.g. `CreateProjectModal`'s current styling, pending DS02.3) still use raw classes as a known, temporary exception.

### Visual direction

- **Aesthetic:** dark-neutral grays + deep crimson primary, Linear/Raycast-inspired — visible borders rather than flat fills, floating depth via shadow/blur rather than flat elevation, full light-mode support via token swap (not a separate design).
- **App shell is chrome-less.** No traditional full-width header/footer bars. Navigation lives in two floating, `rounded-full` pills: a top-right pill (branding + back + theme toggle) and a bottom-center pill nav (60vw wide, capped `max-w-4xl`). Both use `bg-surface-raised/90` + `backdrop-blur-md` + `border-border`, and share an ambient `pillGlow` crimson pulse animation (`@keyframes pillGlow` in `index.css`), paused on hover in favor of a cursor-tracking radial spotlight. Both respect `prefers-reduced-motion` (JS check for the spotlight, `motion-reduce:` variant for the CSS pulse), falling back to a static crimson shadow instead of the animation.
- **Content surfaces (cards, panels) default to a static crimson shadow, not a pulse.** They use `rounded-xl` (a step rounder than the base `rounded-lg`, deliberately short of `rounded-full`) and a static crimson shadow (`shadow-[0_4px_16px_-2px_rgba(185,28,28,0.25)]`, intensifying on hover) — resting content doesn't animate.
- **Pulse is also sanctioned as active/selected-state feedback, not only ambient chrome.** Small selection controls (e.g. `CreateProjectModal`'s color swatches and type-selector buttons) pulse when selected via a tighter `@keyframes swatchGlow` (crisp 2px crimson ring + close blur, ~1.8s) — deliberately louder than a resting card's static shadow because it's marking a live choice, not sitting still. A form panel itself can also pulse via `@keyframes panelGlow` (~3s), which layers the crimson glow *underneath* the existing elevation shadow rather than replacing it, for a livelier feel where wanted (e.g. `CreateProjectModal`). All animated variants (`pillGlow`, `swatchGlow`, `panelGlow`, all in `index.css`) fall back to a static crimson shadow/outline under `prefers-reduced-motion` — never to nothing.
- **Content is centered, not edge-to-edge.** Screens center their content column (`items-center` on a flex container) at a `max-w-2xl` width rather than stretching cards to the viewport edge.
- **Color accents** on list items (e.g. project cards) use a left accent bar reflecting stored entity color, falling back to `primary-800` when unset — not a redundant dot + bar combination.
- **Icons:** [Lucide](https://lucide.dev) (`lucide-react`). Icons default to `stroke="currentColor"` so they inherit text-color utilities automatically — size via `className` (e.g. `h-3.5 w-3.5`), not the `size` prop. Raw emoji icons are being replaced with Lucide as each screen is touched (not a retroactive sweep).
- **Empty states** are styled panels (dashed border, muted icon, heading + hint text) — never a bare emoji + text dump.

---

## 🗂 Document Index

| File | Purpose |
|---|---|
| [MISSION.md](MISSION.md) | This file — goals, pillars, architecture overview |
| [ROADMAP.md](ROADMAP.md) | Phase-by-phase execution plan with completion status |
| [TASKS.md](TASKS.md) | Epic / feature board (sprint view) |
| [TICKETS.md](TICKETS.md) | Active granular tickets for pending epics |
| [CompletedTickets.md](CompletedTickets.md) | Archived tickets for completed epics |
