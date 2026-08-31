namespace TaskManager.Domain.Enums;

/// <summary>
/// Defines the urgency and importance ranking for a <see cref="Entities.ProjectTask"/>.
/// Values are ordered from least to most urgent so that ascending integer comparisons
/// produce a natural low-to-high sort without additional mapping.
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Nice-to-have work; no time pressure. Can be deferred indefinitely.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Standard priority. Should be completed within the current sprint or planning period.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// Elevated urgency. Should be completed before the next planning cycle or review.
    /// </summary>
    High = 2,

    /// <summary>
    /// Blocking or production-impacting work. Must be addressed immediately.
    /// </summary>
    Critical = 3
}
