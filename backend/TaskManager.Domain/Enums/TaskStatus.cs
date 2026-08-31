namespace TaskManager.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a <see cref="Entities.ProjectTask"/>.
/// Values are ordered to reflect a natural progression through a work pipeline,
/// enabling straightforward ordinal comparisons and board column mapping.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// The task has been created but no work has started. Default state on creation.
    /// Maps to the leftmost column in a Kanban/Trello-style board view.
    /// </summary>
    Backlog = 0,

    /// <summary>
    /// The task is actively being worked on by one or more assignees.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Work has started but is halted due to an external dependency or impediment.
    /// Requires explicit resolution before the task can return to <c>InProgress</c>.
    /// </summary>
    Blocked = 2,

    /// <summary>
    /// The task is complete and has been verified or accepted.
    /// Terminal state — no further transitions are expected.
    /// </summary>
    Done = 3,

    /// <summary>
    /// The task was intentionally abandoned or descoped.
    /// Soft-delete equivalent; preserved in history but excluded from active views.
    /// </summary>
    Cancelled = 4
}
