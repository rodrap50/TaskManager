using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a long-horizon grouping of work within a <see cref="Project"/> (e.g., "Q3
/// Onboarding Revamp", "Mobile Parity"). Epics let related tasks be grouped for reporting
/// and filtering without changing where those tasks live in the workflow pipeline.
/// </summary>
/// <remarks>
/// <para>
/// <c>Epic</c> is a child entity of the <see cref="Project"/> aggregate root, and a sibling
/// of <see cref="ProjectPhase"/> — not a replacement for it. A task's phase describes where
/// it is in the workflow; its epic (if any) describes what larger body of work it belongs to.
/// All mutations that affect the parent project's state (e.g., adding/removing an epic)
/// must be routed through <see cref="Project.AddEpic"/> and <see cref="Project.RemoveEpic"/>
/// to preserve aggregate consistency and keep the parent's <see cref="AuditInfo"/> current.
/// </para>
/// <para>
/// <b>Ordering:</b> The <see cref="DisplayOrder"/> property is a zero-based integer that
/// determines rendering order wherever epics are listed (e.g., a picker). The Application
/// layer is responsible for maintaining gap-free, sequential order values when epics are
/// reordered or deleted.
/// </para>
/// </remarks>
public sealed class Epic
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
    /// Human-readable label for this epic (e.g., "Q3 Onboarding Revamp"). Maximum 100
    /// characters.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Zero-based integer controlling the rendering order of epics wherever they are
    /// listed. Managed by the Application service layer.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Optional description providing context about what this epic represents.
    /// Supports short plain text.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Optional six-character hex color string (without leading <c>#</c>) used to visually
    /// distinguish this epic's badge wherever it's displayed. <see langword="null"/> when
    /// no color has been assigned. Mirrors <see cref="Entities.Project.ColorHex"/>.
    /// </summary>
    public string? ColorHex { get; private set; }

    // -------------------------------------------------------------------------
    // Audit
    // -------------------------------------------------------------------------

    /// <summary>
    /// Encapsulates all temporal and optimistic-concurrency metadata.
    /// </summary>
    public AuditInfo Audit { get; private set; }

    // -------------------------------------------------------------------------
    // Navigation (child tasks scoped to this epic)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tasks that have been explicitly assigned to this epic.
    /// A task may belong to a project without belonging to any epic (epic assignment is
    /// optional on <see cref="ProjectTask"/>). Use <see cref="Project.AddTask"/> to create
    /// tasks and the task's own <c>AssignToEpic</c> method to attach them to an epic.
    /// </summary>
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();

    private readonly List<ProjectTask> _tasks;

    // -------------------------------------------------------------------------
    // Construction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises a new <see cref="Epic"/>.
    /// </summary>
    /// <param name="projectId">
    ///   ID of the owning <see cref="Project"/>. Must not be <see cref="Guid.Empty"/>.
    /// </param>
    /// <param name="name">Epic label. Must not be null or whitespace.</param>
    /// <param name="displayOrder">
    ///   Zero-based rendering order. Must be non-negative.
    /// </param>
    /// <param name="description">Optional descriptive text for this epic.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="projectId"/> is empty, <paramref name="name"/> is
    ///   null/whitespace, or <paramref name="displayOrder"/> is negative.
    /// </exception>
    public Epic(
        Guid projectId,
        string name,
        int displayOrder,
        string? description = null,
        string? colorHex = null)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId must be a valid, non-empty GUID.", nameof(projectId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Epic name must not be null or whitespace.", nameof(name));

        if (displayOrder < 0)
            throw new ArgumentException("DisplayOrder must be a non-negative integer.", nameof(displayOrder));

        Id           = Guid.NewGuid();
        ProjectId    = projectId;
        Name         = name.Trim();
        DisplayOrder = displayOrder;
        Description  = description?.Trim();
        ColorHex     = NormalizeColorHex(colorHex);
        Audit        = AuditInfo.CreateNew();
        _tasks       = [];
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private Epic()
    {
        Name  = string.Empty;
        Audit = AuditInfo.CreateNew();
        _tasks = [];
    }

    // -------------------------------------------------------------------------
    // Domain Behaviour
    // -------------------------------------------------------------------------

    /// <summary>
    /// Renames this epic and updates the audit trail.
    /// </summary>
    /// <param name="newName">Must not be null or whitespace.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="newName"/> is null, empty, or whitespace.
    /// </exception>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Epic name must not be null or whitespace.", nameof(newName));

        Name = newName.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Updates the description of this epic.
    /// Pass <see langword="null"/> to clear an existing description.
    /// </summary>
    public void UpdateDescription(string? newDescription)
    {
        Description = string.IsNullOrWhiteSpace(newDescription) ? null : newDescription.Trim();
        Audit = Audit.Touch();
    }

    /// <summary>
    /// Changes the rendering order of this epic.
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
