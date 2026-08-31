# Epic as a first-class entity (child of Project, sibling of Phase)

> **Status: Deferred.** Fully researched and approved on 2026-08-20, but shelved until Sprint 1b's UI/design-system work (DS01–DS03) finishes — this is a full-stack cross-layer change and deserves its own focused session rather than interleaving with UI work. See the Post-MVP Backlog entry in [ROADMAP.md](ROADMAP.md) for tracking. When ready to pick this up, turn it into proper tickets in `TICKETS.md`/`TASKS.md` via the planner tool rather than working straight off this file.

## Context

Today "Epic" isn't a real entity — it's just one value of `Project.Scope` (`ProjectScope.Epic=0, Project=1, DailyTask=2`, in [`backend/TaskManager.Domain/Enums/ProjectScope.cs`](backend/TaskManager.Domain/Enums/ProjectScope.cs)). An "Epic" is currently a `Project` row flagged as the top-level grouping — matching `MISSION.md` Pillar 4's old framing, "macro-to-micro views (Epics → Projects → Daily Tasks)".

The goal is to invert this: `Project` is always the top-level container, and `Epic` becomes a genuine **child entity of Project** — a feature/initiative grouping, independent of the existing `ProjectPhase` entity (which represents workflow columns like Design/Dev/QA, already scoped to one Project). Confirmed via clarifying questions during planning:
- Epic is **independent of Phase** — orthogonal grouping axis, not a replacement and not nested inside it.
- A `ProjectTask` gets a **single optional `EpicId`** FK (not many-to-many), mirroring exactly how `ProjectTask.PhaseId` already works.

`ProjectPhase` already has a complete, proven pattern across every layer (Domain → EF → CQRS → Controller → frontend types/hooks), and nothing in the frontend special-cases `scope === 'Epic'` today (confirmed via repo search — it's purely decorative), so this is a low-risk mirror-the-pattern change, not a redesign.

**Scope boundary:** full backend CRUD for `Epic` (mirroring `Phase`'s complete CRUD) + frontend types/hooks/read-assign wiring in the task detail panel — but **no dedicated "manage Epics" screen** (create/edit/delete UI), matching the exact boundary Phase itself shipped at (Phase still has no create/edit UI in the frontend either, just read + assign). That stays a follow-up ticket.

`MISSION.md` Pillar 4's "Epics → Projects" wording is now backwards and needs updating once this ships — that edit belongs to the planning workflow/skill, not the coding session.

---

## Decisions made while planning (flagging for visibility, not re-asking)

- **Class name: `Epic`**, not `ProjectEpic`. `ProjectPhase`/`ProjectTask` share a "Project" prefix stutter already; `Epic` alone is unambiguous and reads better at call sites (`new Epic(...)`, `IEpicRepository`, `task.AssignToEpic(...)`). Easy to rename later if a prefix-consistency preference emerges.
- **Keep `DisplayOrder`/`Reorder`** on `Epic`, symmetric with `Phase`. Costs one int column now and saves a migration later once an epic-list UI wants stable ordering.
- **`ProjectScope` renumbering requires a data fix.** Removing `Epic=0` and renumbering to `Project=0, DailyTask=1` would silently reinterpret any existing `Scope=1` row (today's `Project`) as `DailyTask`. At planning time the dev DB only had throwaway test projects, so the plan is to drop and let `DataSeeder`/auto-migration recreate the schema from scratch rather than writing a raw-SQL backfill — simpler, and there's no real data to lose. Re-check whether that's still true (no real data accumulated) before executing; write the backfill `UPDATE` instead if it isn't.
- **`frontend/src/` (legacy pre-monorepo tree) is left untouched.** Confirmed dead — not in the `packages/*` workspace, not built, not served. Out of scope; candidate for a separate cleanup ticket.

---

## Implementation

### Domain (`backend/TaskManager.Domain/`)

- **New `Entities/Epic.cs`** — copy `Entities/Phase.cs` (`ProjectPhase`) structure: `Guid Id`, immutable `Guid ProjectId` + `Project? Project` nav, `string Name`, `string? Description`, `int DisplayOrder`, `AuditInfo Audit`, `IReadOnlyCollection<ProjectTask> Tasks`. Public ctor `Epic(Guid projectId, string name, int displayOrder, string? description = null)` with the same validation as `ProjectPhase`; private parameterless ctor for EF. Behavior methods `Rename`, `UpdateDescription`, `Reorder` (identical bodies to `ProjectPhase`, each touching `Audit`).
- **`Entities/Project.cs`** — add `IReadOnlyCollection<Epic> Epics` (backed by private `List<Epic>`), `AddEpic(Epic)` / `RemoveEpic(Guid)` mirroring `AddPhase`/`RemovePhase`.
- **`Entities/TaskItem.cs`** (`ProjectTask`) — add `Guid? EpicId` + `Epic? Epic` nav next to the existing `PhaseId`/`Phase` block; ctor param `Guid? epicId = null` assigned directly (same as `PhaseId`, no empty-guid normalization); new `AssignToEpic(Guid)` / `RemoveFromEpic()` methods mirroring `AssignToPhase`/`RemoveFromPhase`.
- **`Enums/ProjectScope.cs`** — remove `Epic`, renumber explicitly:
  ```csharp
  public enum ProjectScope { Project = 0, DailyTask = 1 }
  ```

### Infrastructure / EF (`backend/TaskManager.Infrastructure/`)

- **`Data/AppDbContext.cs`**: add `DbSet<Epic> Epics`; new `ConfigureEpic(modelBuilder)` (mirrors `ConfigureProjectPhase` — key, maxlengths, owned `Audit`, and the `Epic → Tasks` FK config with `OnDelete(DeleteBehavior.SetNull)`, following the existing convention where the child-FK config lives on the *referenced parent's* configure method); `ConfigureProject` gets a `HasMany(p => p.Epics)...OnDelete(Cascade)` block mirroring `Phases`.
- **New migration**: `dotnet ef migrations add AddEpicEntity` (from `TaskManager.Infrastructure`, startup project `TaskManager.API`) — auto-generated, creates the `Epics` table and the `Tasks.EpicId` FK column.
- **Drop & recreate the dev DB** (per the decision above) so the `ProjectScope` renumbering doesn't corrupt existing rows — `dotnet ef database drop` then let `Program.cs`'s existing auto-migrate-on-startup rebuild it, or `dotnet ef database update`.
- **New `IEpicRepository`** (`Application/Common/Interfaces/`) + **`EpicRepository`** (`Infrastructure/Repositories/`) mirroring `IProjectPhaseRepository`/`ProjectPhaseRepository` exactly, including `GetByProjectAsync`.
- **`IUnitOfWork`** + **`UnitOfWork`** — add `Epics` repository property, constructed in `UnitOfWork`'s ctor like the others. No `Program.cs` DI change needed (only `IUnitOfWork` itself is registered).

### Application layer (`backend/TaskManager.Application/`)

- **New `Epics/` folder** mirroring `Phases/` exactly: `Commands/CreateEpicCommand.cs`, `Commands/UpdateEpicCommand.cs`, `Commands/DeleteEpicCommand.cs`, `Queries/GetEpicsByProjectQuery.cs`, `Validators/CreateEpicCommandValidator.cs` — same handler bodies, same partial-update pattern, same FluentValidation rules as their Phase counterparts.
- **New `Common/DTOs/EpicDto.cs`**: `record EpicDto(Guid Id, Guid ProjectId, string Name, string? Description, int DisplayOrder, DateTime CreatedAt, DateTime? UpdatedAt)`.
- **`Common/DTOs/ProjectDto.cs`** — add `IReadOnlyList<EpicDto> Epics`. **`ProjectTaskDto.cs`** — add `Guid? EpicId`.
- **`Common/Mappings/MappingExtensions.cs`** — add `ToDto(this Epic e)`; extend `Project.ToDto()` to include `Epics`; extend `ProjectTask.ToDto()` to include `EpicId`.
- **`Tasks/Commands/CreateTaskCommand.cs`** — add `Guid? EpicId = null` param (mirrors existing `PhaseId` param), passed into `new ProjectTask(...)`.
- **`Tasks/Commands/UpdateTaskCommand.cs`** — add `Guid? EpicId` + `bool ClearEpic` params, mirroring the existing `PhaseId`/`ClearPhase` handling in the handler body.

### API layer (`backend/TaskManager.API/`)

- **New `Controllers/EpicsController.cs`** — mirror `PhasesController.cs` verbatim (`api/Epics` route: `GET project/{projectId:guid}`, `POST`, `PUT {id:guid}` via an `UpdateEpicRequest` record, `DELETE {id:guid}`).
- **`Controllers/TasksController.cs`** — `UpdateTaskRequest` gets `Guid? EpicId` + `bool ClearEpic` fields, threaded into the `UpdateTaskCommand` construction alongside the existing Phase fields.

### Frontend

- **`frontend/packages/shared/src/ApiClient.ts`**: add `EpicDto`, `CreateEpicRequest`, `UpdateEpicRequest` types; add `epics: EpicDto[]` to `ProjectDto`; add `epicId: string | null` to `ProjectTaskDto`; add `epicId?: string` to `CreateTaskRequest`; add `epicId?: string; clearEpic?: boolean` to `UpdateTaskRequest`; new `getEpicsByProject` / `createEpic` / `updateEpic` / `deleteEpic` functions mirroring the existing Phase functions. **Trim** `CreateProjectRequest.scope` / `UpdateProjectRequest.scope?` from `'Epic' | 'Project' | 'DailyTask'` to `'Project' | 'DailyTask'`.
- **New `frontend/packages/web/src/hooks/useEpics.ts`** — copy `usePhases.ts` verbatim, renamed.
- **`frontend/packages/web/src/screens/BoardScreen.tsx`** — call `useEpics(selectedProjectId)` alongside `usePhases`, fold into the loading state, pass `epics` down to `KanbanBoard`/`ListView`.
- **`frontend/packages/web/src/features/board/ListView.tsx`** — thread `epics: EpicDto[]` through `Props`/`DetailPanelProps`; add an Epic picker block in `TaskDetailPanel` directly below the existing Phase picker, structurally identical (None + one button per epic, calling `updateTask(task.id, { epicId })` / `{ clearEpic: true }`); add an `epicName` badge to `TaskRow` mirroring `phaseName`.
- **`frontend/packages/web/src/features/board/KanbanBoard.tsx`** — check during implementation whether it independently renders a phase badge/picker; if so, thread `epics` through the same way for parity.
- **`frontend/packages/web/src/screens/ProjectSelectorScreen/CreateProjectModal/CreateProjectModal.tsx`** — trim `SCOPE_OPTIONS` from `['Project', 'Epic', 'DailyTask'] as const` to `['Project', 'DailyTask'] as const`. No other change needed — nothing branches on `scope === 'Epic'` anywhere in the frontend.
- `ProjectCard.tsx` needs no change — it just interpolates `project.scope` as text.

---

## Verification

1. **Backend build**: `dotnet build` — confirms Domain/Infrastructure/Application/API compile with the new entity, the `ProjectScope` renumbering, and `ProjectTask.EpicId`.
2. **Migration**: generate with `dotnet ef migrations add AddEpicEntity`; inspect the generated file to confirm it creates the `Epics` table and the `Tasks.EpicId` FK with `SetNull` delete behavior.
3. **DB reset**: `dotnet ef database drop` then let auto-migration (or `dotnet ef database update`) rebuild the schema.
4. **Backend smoke test** (Scalar UI or curl):
   - `POST /api/projects` with `scope: "Epic"` → now rejected (400).
   - `POST /api/epics` with a valid `projectId` → 201 with an `EpicDto`.
   - `GET /api/epics/project/{projectId}` → returns the created epic.
   - `POST /api/tasks` with `epicId` set → returned `ProjectTaskDto.epicId` matches.
   - `PUT /api/tasks/{id}` with `clearEpic: true` → `epicId` becomes `null`.
   - `DELETE /api/epics/{id}` → 204, then re-fetch the task and confirm `epicId` is now `null`.
5. **Frontend build**: `npm run build --workspace=packages/shared` then `--workspace=packages/web` — confirms TS compiles with the new types/hook and trimmed scope union.
6. **Manual UI smoke test** (dev server + browser preview): open a project's board → List view → open a task's detail panel → confirm an "Epic" picker row now appears below "Phase" and correctly assigns/clears via the API (watch network tab for `PUT /api/tasks/{id}` with `epicId`/`clearEpic`). Confirm `CreateProjectModal` no longer offers "Epic" as a project type.
