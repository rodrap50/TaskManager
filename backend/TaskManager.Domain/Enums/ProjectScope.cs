namespace TaskManager.Domain.Enums;

/// <summary>
/// Defines the macro classification / containment scope of a <see cref="Entities.Project"/>.
/// Drives how a project is rendered in the UI (board columns vs. simplified checklist).
/// </summary>
/// <remarks>
/// Long-horizon groupings of work (formerly the <c>Epic</c> value here) are now represented
/// by the <see cref="Entities.Epic"/> child entity instead — an "Epic" is no longer a kind
/// of <see cref="Entities.Project"/>.
/// </remarks>
public enum ProjectScope
{
    /// <summary>
    /// A concrete, bounded deliverable with a defined start and end.
    /// Analogous to a Jira Project or Trello Board. Typically spans days to weeks.
    /// </summary>
    Project = 0,

    /// <summary>
    /// A lightweight, single-day checklist container for ad-hoc or recurring daily tasks.
    /// Analogous to a personal to-do list or daily stand-up action list.
    /// </summary>
    DailyTask = 1
}
