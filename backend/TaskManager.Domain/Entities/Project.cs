using TaskManager.Domain.Enums;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a macro-level work container — the top-level organizational unit in the
/// task management hierarchy. A <c>Project</c> owns <see cref="ProjectPhase"/> groupings
/// and <see cref="ProjectTask"/> items directly.
/// </summary>
/// <remarks>
/// <para>
/// <c>Project</c> is an aggregate root. All mutations are routed through domain methods
/// rather than direct property setters to ensure invariants (e.g., <see cref="Audit"/>
/// is always touched on change) are consistently enforced.
/// </para>
/// <para>
/// <b>Scope:</b> The <see cref="Scope"/> property classifies this container as a
/// <see cref="ProjectScope.Project"/> (concrete deliverable) or a
/// <see cref="ProjectScope.DailyTask"/> (daily checklist). The UI rendering layer reads
/// this to determine board vs. list presentation. Long-horizon groupings of work are
/// represented by the <see cref="Epic"/> child entity, not by a project scope value.
/// </para>
/// <para>
/// <b>Color:</b> An optional six-character hex color code (without leading <c>#</c>)
/// stored as a string. Used purely for visual differentiation in the board UI.
/// No domain logic depends on this value.
/// </para>
/// <para>
/// <b>Child collections:</b> <see cref="Phases"/>, <see cref="Epics"/>, and <see cref="Tasks"/>
/// are exposed as <see cref="IReadOnlyCollection{T}"/> through backing <see cref="List{T}"/>
/// fields. Mutations are channelled through <see cref="AddPhase"/>/<see cref="RemovePhase"/>,
/// <see cref="AddEpic"/>/<see cref="RemoveEpic"/>, and <see cref="AddTask"/>/<see cref="RemoveTask"/>
/// to maintain aggregate consistency.
/// </para>
/// </remarks>
public sealed class Project
{
    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unique surrogate primary key. Generated once at construction; never mutated.
    /// </summary>
    public Guid Id { get; private set; }

    // -------------------------------------------------------------------------
    // Core Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Short, human-readable name of the project (e.g., "Home Assistant Automation").
    /// Maximum 200 characters. Must not be null or whitespace.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Optional longer description providing context, goals, or acceptance criteria.
    /// Supports markdown formatting for rich presentation in the UI.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Classifies this container's scope: Epic, Project, or DailyTask.
    /// Determines the board/list rendering mode used by the frontend.
    /// </summary>
    public ProjectScope Scope { get; private set; }

    /// <summary>
    /// Optional six-character hex color string (without leading <c>#</c>) used to
    /// visually distinguish this project in board and list views. Example: <c>3B82F6</c>.
    /// <see langword="null"/> when no color has been assigned.
    /// </summary>
    public string? ColorHex { get; private set; }

    /// <summary>
    /// Optional target completion date for this project, stored as UTC.
    /// <see langword="null"/> indicates an open-ended or undated project.
    /// </summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>
    /// Indicates whether the project has been archived (soft-deleted from active views).
    /// Archived projects are excluded from default queries but preserved for history.
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Admin-settable weighting factor (1–10) reflecting this project's overall
    /// criticality, folded into every child task's computed weighted priority score
    /// (PRI01). Defaults to <see cref="DefaultCriticalityScore"/> on project creation.
    /// </summary>
    public int CriticalityScore { get; private set; }

    /// <summary>
    /// Neutral-midpoint default applied to <see cref="CriticalityScore"/> on project
    /// creation, before an admin has explicitly set one.
    /// </summary>
    public const int DefaultCriticalityScore = 5;

    // -------------------------------------------------------------------------
    // Ownership
    // -------------------------------------------------------------------------

    /// <summary>
    /// Foreign key referencing the <see cref="AppUser"/> who created this project.
    /// Immutable after construction — ownership does not transfer.
    /// </summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// Navigation property to the creating <see cref="AppUser"/>.
    /// Populated by EF Core when explicitly included in a query.
    /// </summary>
    public AppUser? CreatedBy { get; private set; }

    // -------------------------------------------------------------------------
    // Audit
    // -------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates all temporal and optimistic-concurrency metadata.
    /// </summary>
    public AuditInfo Audit { get; private set; }

    // -------------------------------------------------------------------------
    // Navigation (child collections — owned by this aggregate root)
    // -------------------------------------------------------------------------

    /// <summary>
    /// The ordered set of workflow phases (e.g., "Design", "Development", "Testing")
    /// that segment this project's task pipeline. Exposed as read-only to prevent
    /// external callers from bypassing aggregate consistency; use <see cref="AddPhase"/>
    /// and <see cref="RemovePhase"/> for mutations.
    /// </summary>
    public IReadOnlyCollection<ProjectPhase> Phases => _phases.AsReadOnly();

    /// <summary>
    /// The set of epics (long-horizon groupings of work) defined for this project.
    /// A sibling of <see cref="Phases"/>, not a replacement for it. Exposed as read-only
    /// to prevent external callers from bypassing aggregate consistency; use
    /// <see cref="AddEpic"/> and <see cref="RemoveEpic"/> for mutations.
    /// </summary>
    public IReadOnlyCollection<Epic> Epics => _epics.AsReadOnly();

    /// <summary>
    /// All tasks belonging directly to this project (not scoped to a phase).
    /// Phase-scoped tasks are also accessible via <see cref="Phases"/>.
    /// Use <see cref="AddTask"/> and <see cref="RemoveTask"/> for mutations.
    /// </summary>
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    /// <summary>
    /// Flat membership roster for this project. Use <see cref="AddMember"/> and
    /// <see cref="RemoveMember"/> for mutations — a project's creator is always the first
    /// row here (see PM01.2).
    /// </summary>
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

    private readonly List<ProjectPhase> _phases;
    private readonly List<Epic> _epics;
    private readonly List<ProjectTask> _tasks;
    private readonly List<ProjectMember> _members;

    // -------------------------------------------------------------------------
    // Construction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises a new <see cref="Project"/>.
    /// </summary>
    /// <param name="name">Display name. Must not be null or whitespace.</param>
    /// <param name="scope">Macro classification of this project container.</param>
    /// <param name="createdByUserId">
    ///   ID of the <see cref="AppUser"/> who is creating this project.
    /// </param>
    /// <param name="description">Optional longer description.</param>
    /// <param name="colorHex">
    ///   Optional six-character hex color code (without <c>#</c>).
    /// </param>
    /// <param name="dueDate">Optional UTC due date.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="name"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="createdByUserId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    public Project(
        string name,
        ProjectScope scope,
        Guid createdByUserId,
        string? description = null,
        string? colorHex = null,
        DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name must not be null or whitespace.", nameof(name));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId must be a valid, non-empty GUID.", nameof(createdByUserId));

        Id                = Guid.NewGuid();
        Name              = name.Trim();
        Scope             = scope;
        CreatedByUserId   = createdByUserId;
        Description       = description?.Trim();
        ColorHex          = NormalizeColorHex(colorHex);
        DueDate           = dueDate.HasValue ? DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc) : null;
        IsArchived        = false;
        CriticalityScore  = DefaultCriticalityScore;
        Audit             = AuditInfo.CreateNew();
        _phases           = [];
        _epics            = [];
        _tasks            = [];
        _members          = [];
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private Project()
    {
        Name   = string.Empty;
        Audit  = AuditInfo.CreateNew();
        _phases = [];
        _epics  = [];
        _tasks  = [];
        _members = [];
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Core properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Renames the project and updates the audit trail.
    /// </summary>
    /// <param name="newName">Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="newName"/> is null, empty, or whitespace.
    /// </exception>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Project name must not be null or whitespace.", nameof(newName));

        Name = newName.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Replaces the project description and updates the audit trail.
    /// Pass <see langword="null"/> to clear an existing description.
    /// </summary>
    public void UpdateDescription(string? newDescription)
    {
        Description = string.IsNullOrWhiteSpace(newDescription) ? null : newDescription.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Changes the project's macro scope classification.
    /// </summary>
    /// <param name="newScope">The new <see cref="ProjectScope"/> value.</param>
    public void ChangeScope(ProjectScope newScope)
    {
        Scope = newScope;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Sets or clears the project's accent color.
    /// </summary>
    /// <param name="colorHex">
    ///   Six-character hex string (with or without a leading <c>#</c>), or
    ///   <see langword="null"/> to clear the color.
    /// </param>
    public void SetColor(string? colorHex)
    {
        ColorHex = NormalizeColorHex(colorHex);
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Sets or clears the project's target due date.
    /// </summary>
    /// <param name="dueDate">UTC due date, or <see langword="null"/> to clear.</param>
    public void SetDueDate(DateTime? dueDate)
    {
        DueDate = dueDate.HasValue ? DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc) : null;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Admin-only mutation (enforced by the caller, e.g. PRI01.4's endpoint) that updates
    /// this project's overall criticality weighting, folded into every child task's
    /// computed weighted score.
    /// </summary>
    /// <param name="newScore">Replacement criticality rating, 1–10.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   Thrown when <paramref name="newScore"/> is outside 1–10.
    /// </exception>
    public void SetCriticalityScore(int newScore)
    {
        if (newScore is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(newScore), newScore, "CriticalityScore must be between 1 and 10.");

        CriticalityScore = newScore;
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Archival
    // -------------------------------------------------------------------------

    /// <summary>
    /// Marks the project as archived, hiding it from active board and list views.
    /// All child tasks and phases are preserved.
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Restores an archived project to active status.
    /// </summary>
    public void Restore()
    {
        IsArchived = false;
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Phase management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a new <see cref="ProjectPhase"/> as a child of this project.
    /// The phase's <c>ProjectId</c> must already be set to this project's <see cref="Id"/>;
    /// this is enforced by the <see cref="ProjectPhase"/> constructor.
    /// </summary>
    /// <param name="phase">The phase to register. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="phase"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the phase does not belong to this project.
    /// </exception>
    public void AddPhase(ProjectPhase phase)
    {
        ArgumentNullException.ThrowIfNull(phase);

        if (phase.ProjectId != Id)
            throw new ArgumentException(
                $"Phase '{phase.Name}' belongs to project '{phase.ProjectId}', not '{Id}'.",
                nameof(phase));

        _phases.Add(phase);
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes a <see cref="ProjectPhase"/> from this project by its ID.
    /// Does nothing if the phase is not found (idempotent).
    /// </summary>
    /// <param name="phaseId">ID of the phase to remove.</param>
    public void RemovePhase(Guid phaseId)
    {
        var phase = _phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase is null) return;

        _phases.Remove(phase);
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Epic management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a new <see cref="Epic"/> as a child of this project.
    /// The epic's <c>ProjectId</c> must already be set to this project's <see cref="Id"/>;
    /// this is enforced by the <see cref="Epic"/> constructor.
    /// </summary>
    /// <param name="epic">The epic to register. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="epic"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the epic does not belong to this project.
    /// </exception>
    public void AddEpic(Epic epic)
    {
        ArgumentNullException.ThrowIfNull(epic);

        if (epic.ProjectId != Id)
            throw new ArgumentException(
                $"Epic '{epic.Name}' belongs to project '{epic.ProjectId}', not '{Id}'.",
                nameof(epic));

        _epics.Add(epic);
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes an <see cref="Epic"/> from this project by its ID.
    /// Does nothing if the epic is not found (idempotent).
    /// </summary>
    /// <param name="epicId">ID of the epic to remove.</param>
    public void RemoveEpic(Guid epicId)
    {
        var epic = _epics.FirstOrDefault(e => e.Id == epicId);
        if (epic is null) return;

        _epics.Remove(epic);
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Task management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a new <see cref="ProjectTask"/> as a direct child of this project.
    /// </summary>
    /// <param name="task">The task to register. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="task"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the task's <c>ProjectId</c> does not match this project's <see cref="Id"/>.
    /// </exception>
    public void AddTask(ProjectTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.ProjectId != Id)
            throw new ArgumentException(
                $"Task '{task.Title}' belongs to project '{task.ProjectId}', not '{Id}'.",
                nameof(task));

        _tasks.Add(task);
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes a <see cref="ProjectTask"/> from this project by its ID.
    /// Does nothing if the task is not found (idempotent).
    /// </summary>
    /// <param name="taskId">ID of the task to remove.</param>
    public void RemoveTask(Guid taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null) return;

        _tasks.Remove(task);
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Membership management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds <paramref name="userId"/> to this project's membership roster, constructing
    /// the <see cref="ProjectMember"/> row internally. Idempotent — does nothing if the
    /// user is already a member.
    /// </summary>
    /// <param name="userId">ID of the <see cref="AppUser"/> to add. Must not be empty.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="userId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    public void AddMember(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must be a valid, non-empty GUID.", nameof(userId));

        if (_members.Any(m => m.UserId == userId))
            return;

        _members.Add(new ProjectMember(Id, userId));
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes a member from this project by user ID.
    /// Does nothing if the user is not a member (idempotent).
    /// </summary>
    /// <param name="userId">ID of the <see cref="AppUser"/> to remove.</param>
    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null) return;

        _members.Remove(member);
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Strips any leading <c>#</c> from a color hex string and uppercases it.
    /// Returns <see langword="null"/> when the input is null or whitespace.
    /// </summary>
    private static string? NormalizeColorHex(string? colorHex)
    {
        if (string.IsNullOrWhiteSpace(colorHex)) return null;

        var trimmed = colorHex.Trim().TrimStart('#').ToUpperInvariant();
        return trimmed.Length is 3 or 6 ? trimmed : null;
    }
}
