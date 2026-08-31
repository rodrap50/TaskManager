using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents an ordered workflow phase within a <see cref="Project"/> (e.g., "Design",
/// "Development", "QA", "Done"). Phases segment a project's task pipeline into logical
/// swimlanes rendered as board columns or list sections in the UI.
/// </summary>
/// <remarks>
/// <para>
/// <c>ProjectPhase</c> is a child entity of the <see cref="Project"/> aggregate root.
/// All mutations that affect the parent project's state (e.g., adding/removing a phase)
/// must be routed through <see cref="Project.AddPhase"/> and <see cref="Project.RemovePhase"/>
/// to preserve aggregate consistency and keep the parent's <see cref="AuditInfo"/> current.
/// </para>
/// <para>
/// <b>Ordering:</b> The <see cref="DisplayOrder"/> property is a zero-based integer that
/// determines left-to-right column order in the board view. The Application layer (B04)
/// is responsible for maintaining gap-free, sequential order values when phases are
/// reordered or deleted.
/// </para>
/// </remarks>
public sealed class ProjectPhase
{
    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unique surrogate primary key. Generated once at construction; never mutated.
    /// </summary>
    public Guid Id { get; private set; }

    // -------------------------------------------------------------------------
    // Relationship
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

    // -------------------------------------------------------------------------
    // Core Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Human-readable label for this phase displayed as the column/section header
    /// (e.g., "Backlog", "In Progress", "Review", "Done"). Maximum 100 characters.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Zero-based integer controlling the left-to-right rendering order of phase columns.
    /// Lower values appear further left. Managed by the Application service layer.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Optional description providing context about what this phase represents
    /// (e.g., "Tasks awaiting developer assignment"). Supports short plain text.
    /// </summary>
    public string? Description { get; private set; }

    // -------------------------------------------------------------------------
    // Audit
    // -------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates all temporal and optimistic-concurrency metadata.
    /// </summary>
    public AuditInfo Audit { get; private set; }

    // -------------------------------------------------------------------------
    // Navigation (child tasks scoped to this phase)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tasks that have been explicitly assigned to this phase.
    /// A task may belong to a project without belonging to any phase (phase is optional
    /// on <see cref="ProjectTask"/>). Use <see cref="Project.AddTask"/> to create tasks
    /// and the task's own <c>AssignToPhase</c> method to move them into a phase.
    /// </summary>
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    private readonly List<ProjectTask> _tasks;

    // -------------------------------------------------------------------------
    // Construction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises a new <see cref="ProjectPhase"/>.
    /// </summary>
    /// <param name="projectId">
    ///   ID of the owning <see cref="Project"/>. Must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="name">Phase label. Must not be null or whitespace.</param>
    /// <param name="displayOrder">
    ///   Zero-based column order. Must be non-negative.
    /// </param>
    /// <param name="description">Optional descriptive text for this phase.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="projectId"/> is empty, <paramref name="name"/> is
    ///   null/whitespace, or <paramref name="displayOrder"/> is negative.
    /// </exception>
    public ProjectPhase(
        Guid projectId,
        string name,
        int displayOrder,
        string? description = null)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId must be a valid, non-empty GUID.", nameof(projectId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Phase name must not be null or whitespace.", nameof(name));

        if (displayOrder < 0)
            throw new ArgumentException("DisplayOrder must be a non-negative integer.", nameof(displayOrder));

        Id           = Guid.NewGuid();
        ProjectId    = projectId;
        Name         = name.Trim();
        DisplayOrder = displayOrder;
        Description  = description?.Trim();
        Audit        = AuditInfo.CreateNew();
        _tasks       = [];
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private ProjectPhase()
    {
        Name  = string.Empty;
        Audit = AuditInfo.CreateNew();
        _tasks = [];
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour
    // -------------------------------------------------------------------------

    /// <summary>
    /// Renames this phase and updates the audit trail.
    /// </summary>
    /// <param name="newName">Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="newName"/> is null, empty, or whitespace.
    /// </exception>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Phase name must not be null or whitespace.", nameof(newName));

        Name = newName.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Updates the description of this phase.
    /// Pass <see langword="null"/> to clear an existing description.
    /// </summary>
    public void UpdateDescription(string? newDescription)
    {
        Description = string.IsNullOrWhiteSpace(newDescription) ? null : newDescription.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Changes the column rendering order of this phase.
    /// </summary>
    /// <param name="newOrder">Must be a non-negative integer.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="newOrder"/> is negative.
    /// </exception>
    public void Reorder(int newOrder)
    {
        if (newOrder < 0)
            throw new ArgumentException("DisplayOrder must be a non-negative integer.", nameof(newOrder));

        DisplayOrder = newOrder;
        Audit = Audit.Touch();
    }
}
