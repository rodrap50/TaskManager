# TaskManager Frontend

npm workspaces monorepo for the HomeAssistant Project Tracker UI.

## Packages

| Package | Description |
|---|---|
| [`packages/shared`](packages/shared) | `@taskmanager/shared` — axios API client + all TypeScript DTOs |
| [`packages/ui`](packages/ui) | `@taskmanager/ui` — presentational React components (TaskCard, PriorityBadge, etc.) |
| [`packages/web`](packages/web) | `@taskmanager/web` — always-online WebUI (MVP target) |
| [`packages/mobile`](packages/mobile) | `@taskmanager/mobile` — Capacitor + offline-first SQLite sync (post-MVP) |

## Dev

```bash
# Install all workspace dependencies
npm install

# Start the web app (port 5173)
npm run dev

# Or explicitly
npm run dev --workspace=packages/web
```

## Architecture

`packages/web` makes direct API calls via `@taskmanager/shared` — no SQLite, no Capacitor, no WASM. It imports visual components from `@taskmanager/ui`.

`packages/mobile` preserves the offline-first Capacitor + `jeep-sqlite` + mutation queue sync work. It is not active for MVP but is fully preserved for the mobile app phase.

Vite workspace aliases in `packages/web/vite.config.ts` resolve `@taskmanager/shared` and `@taskmanager/ui` directly from their `src/` directories (no build step required during development).

## Stack

- React 19 + TypeScript
- Vite 6
- Tailwind CSS v4
- axios (via `@taskmanager/shared`)
- Capacitor 6 + jeep-sqlite (mobile package only)
