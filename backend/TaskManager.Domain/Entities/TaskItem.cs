using TaskManager.Domain.Enums;
using TaskManager.Domain.Services;
using TaskManager.Domain.ValueObjects;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a single, granular, trackable unit of work within a <see cref="Project"/>.
/// This is the primary entity that users create, assign, prioritize, and move through
/// the workflow pipeline.
/// </summary>
/// <remarks>
/// <para>
/// <c>ProjectTask</c> is a child entity of the <see cref="Project"/> aggregate root.
/// All creation must go through <see cref="Project.AddTask"/> to maintain aggregate
/// consistency. Direct instantiation of this entity outside of domain factory methods
/// or the owning aggregate is not permitted in application code.
/// </para>
/// <para>
/// <b>Assignment model:</b> A task supports a single primary assignee
/// (<see cref="AssignedUserId"/> / <see cref="AssignedUser"/>) and an optional
/// secondary assignee (<see cref="SecondaryAssigneeId"/> / <see cref="SecondaryAssignee"/>)
/// to cover pair-programming or review workflows without over-engineering a full
/// many-to-many assignment table at this stage. The architecture can be extended to
/// many-to-many in B02 if the requirement evolves.
/// </para>
/// <para>
/// <b>Metadata dictionary:</b> <see cref="ExternalMetadata"/> is a string-to-string
/// dictionary serialized as JSON in the database (B02 Fluent API). It stores opaque
/// footprint data from external tools — for example, a Home Assistant entity ID, an
/// MQTT topic, or an inbound webhook reference ID — without polluting the domain model
/// with integration-specific columns.
/// </para>
/// <para>
/// <b>Status vs. IsCompleted:</b> The old boolean <c>IsCompleted</c> field from the
/// prototype has been superseded by the <see cref="TaskStatus"/> enum, which provides
/// a richer, Jira-aligned pipeline. The <see cref="IsCompleted"/> computed property is
/// retained as a convenience accessor for the UI's simple checkbox rendering path.
/// </para>
/// </remarks>
public sealed class ProjectTask
{
    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unique surrogate primary key. Generated once at construction; never mutated.
    /// </summary>
    public Guid Id { get; private set; }

    // -------------------------------------------------------------------------
    // Relationships
    // -------------------------------------------------------------------------

    /// <summary>
    /// Foreign key referencing the owning <see cref="Project"/>.
    /// Set at construction and immutable thereafter.
    /// </summary>
    public Guid ProjectId { get; private set; }

    /// <summary>
    /// Navigation property to the owning <see cref="Project"/>.
    /// Populated by EF Core when the relationship is explicitly included in a query.
    /// </summary>
    public Project? Project { get; private set; }

    /// <summary>
    /// Optional foreign key referencing the <see cref="ProjectPhase"/> this task is
    /// currently assigned to. <see langword="null"/> when the task is in the project
    /// backlog and not yet placed in a phase column.
    /// </summary>
    public Guid? PhaseId { get; private set; }

    /// <summary>
    /// Navigation property to the current <see cref="ProjectPhase"/>.
    /// Populated by EF Core when the relationship is explicitly included in a query.
    /// </summary>
    public ProjectPhase? Phase { get; private set; }

    /// <summary>
    /// Optional foreign key referencing the <see cref="Entities.Epic"/> this task belongs
    /// to. <see langword="null"/> when the task is not associated with any epic — epic
    /// assignment is optional, exactly like phase assignment.
    /// </summary>
    public Guid? EpicId { get; private set; }

    /// <summary>
    /// Navigation property to the associated <see cref="Entities.Epic"/>.
    /// Populated by EF Core when the relationship is explicitly included in a query.
    /// </summary>
    public Epic? Epic { get; private set; }

    // -------------------------------------------------------------------------
    // Assignment
    // -------------------------------------------------------------------------

    /// <summary>
    /// Optional foreign key referencing the primary <see cref="AppUser"/> responsible
    /// for completing this task. <see langword="null"/> when the task is unassigned.
    /// </summary>
    public Guid? AssignedUserId { get; private set; }

    /// <summary>
    /// Navigation property to the primary assignee.
    /// Populated by EF Core when the relationship is explicitly included in a query.
    /// </summary>
    public AppUser? AssignedUser { get; private set; }

    /// <summary>
    /// Optional foreign key referencing a secondary <see cref="AppUser"/> (e.g., a
    /// reviewer or pair-programmer). <see langword="null"/> when not applicable.
    /// </summary>
    public Guid? SecondaryAssigneeId { get; private set; }

    /// <summary>
    /// Navigation property to the secondary assignee.
    /// Populated by EF Core when the relationship is explicitly included in a query.
    /// </summary>
    public AppUser? SecondaryAssignee { get; private set; }

    // -------------------------------------------------------------------------
    // Core Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Short, scannable title summarising the work (e.g., "Fix login redirect bug").
    /// Maximum 300 characters. Must not be null or whitespace.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Optional detailed description, acceptance criteria, or notes.
    /// Supports markdown for rich rendering in the task detail panel.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Lifecycle state of this task within the workflow pipeline.
    /// Defaults to <see cref="TaskStatus.Backlog"/> on creation.
    /// Use <see cref="Transition"/> to change state with invariant enforcement.
    /// </summary>
    public TaskStatus Status { get; private set; }

    /// <summary>
    /// Urgency and importance classification.
    /// Defaults to <see cref="TaskPriority.Medium"/> on creation.
    /// </summary>
    public TaskPriority Priority { get; private set; }

    /// <summary>
    /// Optional target completion date stored in UTC.
    /// <see langword="null"/> indicates no deadline has been set.
    /// </summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>
    /// Optional estimated effort in hours. Used for sprint planning and reporting.
    /// <see langword="null"/> when no estimate has been provided.
    /// Must be a positive value when set.
    /// </summary>
    public decimal? EstimatedHours { get; private set; }

    /// <summary>
    /// Convenience computed property. Returns <see langword="true"/> when
    /// <see cref="Status"/> is <see cref="TaskStatus.Done"/> or
    /// <see cref="TaskStatus.Cancelled"/>.
    /// Read-only; drive state through <see cref="Transition"/> instead.
    /// </summary>
    public bool IsCompleted => Status is TaskStatus.Done or TaskStatus.Cancelled;

    /// <summary>
    /// Computed priority score (1–10), blending <see cref="Priority"/>, cast
    /// <see cref="TaskVote"/>s, and the parent project's <c>CriticalityScore</c> via
    /// <see cref="WeightedScoreCalculator"/> (PRI01). Persisted rather than computed
    /// on-read so it can be sorted/filtered on efficiently; kept current by
    /// <see cref="RecalculateWeightedScore"/>, which callers invoke after any input to the
    /// formula changes (priority, a vote, or the project's criticality score).
    /// </summary>
    public int WeightedScore { get; private set; }

    // -------------------------------------------------------------------------
    // External Integration Metadata
    // -------------------------------------------------------------------------

    /// <summary>
    /// A flexible string-to-string dictionary for storing opaque footprint data from
    /// external tools (e.g., Home Assistant entity IDs, MQTT topics, inbound webhook
    /// reference IDs). Serialized as a JSON column in PostgreSQL (B02).
    /// </summary>
    /// <example>
    /// <code>
    /// {
    ///   "ha_entity_id": "input_boolean.task_42_complete",
    ///   "mqtt_topic": "home/tasks/42/status",
    ///   "webhook_source": "home-assistant-automation-v2"
    /// }
    /// </code>
    /// </example>
    public Dictionary<string, string> ExternalMetadata { get; private set; }

    // -------------------------------------------------------------------------
    // Voting
    // -------------------------------------------------------------------------

    /// <summary>
    /// Per-user priority votes cast against this task (PRI01), feeding into its computed
    /// weighted score. Exposed as read-only to prevent external callers from bypassing the
    /// one-vote-per-user invariant; use <see cref="CastVote"/> for mutations.
    /// </summary>
    public IReadOnlyCollection<TaskVote> Votes => _votes.AsReadOnly();

    private readonly List<TaskVote> _votes;

    // -------------------------------------------------------------------------
    // Audit
    // -------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates all temporal and optimistic-concurrency metadata.
    /// </summary>
    public AuditInfo Audit { get; private set; }

    // -------------------------------------------------------------------------
    // Construction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises a new <see cref="ProjectTask"/>.
    /// </summary>
    /// <param name="projectId">
    ///   ID of the owning <see cref="Project"/>. Must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="title">Task title. Must not be null or whitespace.</param>
    /// <param name="priority">
    ///   Urgency classification. Defaults to <see cref="TaskPriority.Medium"/>.
    /// </param>
    /// <param name="description">Optional detailed description.</param>
    /// <param name="phaseId">
    ///   Optional ID of the initial <see cref="ProjectPhase"/>. Pass
    ///   <see langword="null"/> to leave the task in the unphased backlog.
    /// </param>
    /// <param name="assignedUserId">
    ///   Optional ID of the primary assignee. Pass <see langword="null"/> to leave
    ///   the task unassigned.
    /// </param>
    /// <param name="dueDate">Optional UTC due date.</param>
    /// <param name="estimatedHours">Optional effort estimate in hours.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="projectId"/> is empty, <paramref name="title"/> is
    ///   null/whitespace, or <paramref name="estimatedHours"/> is non-positive.
    /// </exception>
    public ProjectTask(
        Guid projectId,
        string title,
        TaskPriority priority = TaskPriority.Medium,
        string? description = null,
        Guid? phaseId = null,
        Guid? assignedUserId = null,
        DateTime? dueDate = null,
        decimal? estimatedHours = null)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId must be a valid, non-empty GUID.", nameof(projectId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title must not be null or whitespace.", nameof(title));

        if (estimatedHours.HasValue && estimatedHours.Value <= 0)
            throw new ArgumentException("EstimatedHours must be a positive value when specified.", nameof(estimatedHours));

        Id               = Guid.NewGuid();
        ProjectId        = projectId;
        PhaseId          = phaseId;
        AssignedUserId   = assignedUserId == Guid.Empty ? null : assignedUserId;
        Title            = title.Trim();
        Description      = description?.Trim();
        Status           = TaskStatus.Backlog;
        Priority         = priority;
        DueDate          = dueDate.HasValue ? DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc) : null;
        EstimatedHours   = estimatedHours;
        ExternalMetadata = [];
        Audit            = AuditInfo.CreateNew();
        _votes           = [];
        WeightedScore    = WeightedScoreCalculator.Compute(priority, [], Project.DefaultCriticalityScore);
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private ProjectTask()
    {
        Title            = string.Empty;
        ExternalMetadata = [];
        Audit            = AuditInfo.CreateNew();
        _votes           = [];
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Core mutations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Updates the task title and records the mutation timestamp.
    /// </summary>
    /// <param name="newTitle">Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="newTitle"/> is null, empty, or whitespace.
    /// </exception>
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Task title must not be null or whitespace.", nameof(newTitle));

        Title = newTitle.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Replaces the task description and updates the audit trail.
    /// Pass <see langword="null"/> to clear an existing description.
    /// </summary>
    public void UpdateDescription(string? newDescription)
    {
        Description = string.IsNullOrWhiteSpace(newDescription) ? null : newDescription.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Changes the task's urgency classification.
    /// </summary>
    /// <param name="newPriority">The replacement <see cref="TaskPriority"/> value.</param>
    public void Reprioritize(TaskPriority newPriority)
    {
        Priority = newPriority;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Transitions the task to a new workflow state.
    /// </summary>
    /// <remarks>
    /// <b>Allowed transitions enforced here:</b>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       A <see cref="TaskStatus.Done"/> or <see cref="TaskStatus.Cancelled"/> task
    ///       cannot be transitioned to any state other than <see cref="TaskStatus.Backlog"/>
    ///       (i.e., explicitly reopened). This prevents silent re-opening via state leakage.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Transitioning to the same state is a no-op (idempotent) rather than an error,
    ///       which aligns with offline-sync replay semantics.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="newStatus">The target workflow state.</param>
    /// <exception cref="InvalidOperationException">
    ///   Thrown when attempting to transition a completed/cancelled task to a state other
    ///   than <see cref="TaskStatus.Backlog"/>.
    /// </exception>
    public void Transition(TaskStatus newStatus)
    {
        // Idempotent no-op
        if (Status == newStatus) return;

        // Terminal state guard: completed or cancelled tasks can only be explicitly reopened
        if (IsCompleted && newStatus != TaskStatus.Backlog)
        {
            throw new InvalidOperationException(
                $"Cannot transition task '{Id}' from '{Status}' to '{newStatus}'. " +
                $"A completed or cancelled task can only be re-opened to '{TaskStatus.Backlog}'.");
        }

        Status = newStatus;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Sets or updates the due date of this task.
    /// </summary>
    /// <param name="dueDate">UTC due date, or <see langword="null"/> to clear.</param>
    public void SetDueDate(DateTime? dueDate)
    {
        DueDate = dueDate.HasValue ? DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc) : null;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Sets or updates the estimated effort for this task.
    /// </summary>
    /// <param name="hours">
    ///   Positive decimal representing hours, or <see langword="null"/> to clear.
    /// </param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="hours"/> is zero or negative.
    /// </exception>
    public void SetEstimatedHours(decimal? hours)
    {
        if (hours.HasValue && hours.Value <= 0)
            throw new ArgumentException("EstimatedHours must be a positive value when specified.", nameof(hours));

        EstimatedHours = hours;
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Assignment
    // -------------------------------------------------------------------------

    /// <summary>
    /// Assigns or re-assigns the primary user responsible for this task.
    /// </summary>
    /// <param name="userId">
    ///   ID of the <see cref="AppUser"/> to assign as the primary owner.
    ///   Must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="userId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    public void AssignTo(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must be a valid, non-empty GUID.", nameof(userId));

        AssignedUserId = userId;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes the primary user assignment, leaving the task unassigned.
    /// </summary>
    public void Unassign()
    {
        AssignedUserId = null;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Sets or replaces the secondary assignee (e.g., a reviewer or pair-programmer).
    /// </summary>
    /// <param name="userId">
    ///   ID of the <see cref="AppUser"/> to assign as secondary.
    ///   Must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="userId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    public void SetSecondaryAssignee(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must be a valid, non-empty GUID.", nameof(userId));

        SecondaryAssigneeId = userId;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes the secondary assignee.
    /// </summary>
    public void ClearSecondaryAssignee()
    {
        SecondaryAssigneeId = null;
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Phase placement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves this task into the specified workflow phase (e.g., from backlog into
    /// "In Progress" column). The phase must belong to the same parent project.
    /// </summary>
    /// <param name="phaseId">
    ///   ID of the target <see cref="ProjectPhase"/>. Must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="phaseId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    public void AssignToPhase(Guid phaseId)
    {
        if (phaseId == Guid.Empty)
            throw new ArgumentException("PhaseId must be a valid, non-empty GUID.", nameof(phaseId));

        PhaseId = phaseId;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes this task from its current phase, returning it to the unphased backlog.
    /// </summary>
    public void RemoveFromPhase()
    {
        PhaseId = null;
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Epic association
    // -------------------------------------------------------------------------

    /// <summary>
    /// Associates this task with the specified epic (e.g., grouping it under a larger
    /// body of work). The epic must belong to the same parent project.
    /// </summary>
    /// <param name="epicId">
    ///   ID of the target <see cref="Entities.Epic"/>. Must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="epicId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    public void AssignToEpic(Guid epicId)
    {
        if (epicId == Guid.Empty)
            throw new ArgumentException("EpicId must be a valid, non-empty GUID.", nameof(epicId));

        EpicId = epicId;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes this task's association with its current epic, if any.
    /// </summary>
    public void RemoveFromEpic()
    {
        EpicId = null;
        Audit = Audit.Touch();
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — Voting
    // -------------------------------------------------------------------------

    /// <summary>
    /// Casts or updates <paramref name="userId"/>'s priority vote for this task. If the
    /// user has already voted, their existing <see cref="TaskVote"/> is updated in place
    /// rather than a duplicate row being inserted (enforced in-memory here, mirrored by a
    /// unique DB constraint on <c>(TaskId, UserId)</c>).
    /// </summary>
    /// <param name="userId">ID of the voting <see cref="AppUser"/>. Must not be empty.</param>
    /// <param name="voteValue">Priority rating, 1–10.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="userId"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   Thrown when <paramref name="voteValue"/> is outside 1–10.
    /// </exception>
    public void CastVote(Guid userId, int voteValue)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must be a valid, non-empty GUID.", nameof(userId));

        var existing = _votes.FirstOrDefault(v => v.UserId == userId);
        if (existing is not null)
        {
            existing.UpdateValue(voteValue);
        }
        else
        {
            _votes.Add(new TaskVote(Id, userId, voteValue));
        }

        Audit = Audit.Touch();
    }

    /// <summary>
    /// Recomputes and persists <see cref="WeightedScore"/> from this task's current
    /// <see cref="Priority"/> and <see cref="Votes"/>, combined with the parent project's
    /// criticality score (passed in rather than read from <see cref="Project"/>, since that
    /// navigation isn't always loaded — the caller is responsible for supplying the current
    /// value). Callers invoke this after casting/changing a vote (PRI01.3), changing
    /// <see cref="Priority"/>, or changing the project's <c>CriticalityScore</c> (PRI01.4).
    /// </summary>
    /// <param name="criticalityScore">
    ///   The current <see cref="Project.CriticalityScore"/> of this task's parent project.
    /// </param>
    public void RecalculateWeightedScore(int criticalityScore)
    {
        WeightedScore = WeightedScoreCalculator.Compute(
            Priority,
            _votes.Select(v => v.VoteValue).ToList(),
            criticalityScore);
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour — External metadata
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds or overwrites a key-value pair in the external metadata dictionary.
    /// Used by the Integration Gateway (B05) to stamp tasks with tool-specific
    /// reference data (e.g., Home Assistant entity IDs, MQTT topics).
    /// </summary>
    /// <param name="key">Case-sensitive metadata key. Must not be null or whitespace.</param>
    /// <param name="value">String value to store. Must not be null.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="key"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public void SetMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key must not be null or whitespace.", nameof(key));

        ArgumentNullException.ThrowIfNull(value);

        ExternalMetadata[key] = value;
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Removes a key from the external metadata dictionary.
    /// Does nothing if the key does not exist (idempotent).
    /// </summary>
    /// <param name="key">The key to remove.</param>
    public void RemoveMetadata(string key)
    {
        if (ExternalMetadata.Remove(key))
            Audit = Audit.Touch();
    }

    /// <summary>
    /// Clears all entries from the external metadata dictionary.
    /// </summary>
    public void ClearMetadata()
    {
        if (ExternalMetadata.Count == 0) return;

        ExternalMetadata.Clear();
        Audit = Audit.Touch();
    }
}
