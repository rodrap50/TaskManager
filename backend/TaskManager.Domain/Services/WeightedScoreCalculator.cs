using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Services;

/// <summary>
/// Pure, stateless implementation of the weighted-priority-score formula (PRI01) — the
/// single place this calculation is implemented, so every caller (task creation, priority
/// changes, vote casts, admin criticality changes) computes it identically.
/// </summary>
/// <remarks>
/// Lives in the Domain layer (rather than Application, per PRI01.2's "or equivalent domain
/// service" allowance) so <see cref="Entities.ProjectTask.RecalculateWeightedScore"/> can
/// call it directly without inverting the Domain → Application dependency direction.
/// </remarks>
public static class WeightedScoreCalculator
{
    /// <summary>
    /// Maps the manually-set <see cref="TaskPriority"/> enum onto the same 1–10 scale used
    /// by votes and <see cref="Entities.Project.CriticalityScore"/>.
    /// </summary>
    public static int PriorityToNumeric(TaskPriority priority) => priority switch
    {
        TaskPriority.Low      => 3,
        TaskPriority.Medium   => 7,
        TaskPriority.High     => 9,
        TaskPriority.Critical => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Unrecognized TaskPriority value."),
    };

    /// <summary>
    /// Ceiling of the arithmetic mean of all cast vote values for a task. When no votes
    /// have been cast, falls back to the task's own <see cref="PriorityToNumeric"/> value —
    /// an un-voted task's aggregate opinion defaults to its stated priority rather than an
    /// arbitrary neutral midpoint (implementer's call, per PRI01.2's ticket notes).
    /// </summary>
    public static int ComputeAvgUserVote(IReadOnlyCollection<int> voteValues, TaskPriority priority)
    {
        if (voteValues.Count == 0)
            return PriorityToNumeric(priority);

        return (int)Math.Ceiling(voteValues.Average());
    }

    /// <summary>
    /// Computes the final weighted priority score: the ceiling of the mean of
    /// <see cref="PriorityToNumeric"/>, <see cref="ComputeAvgUserVote"/>, and the parent
    /// project's <see cref="Entities.Project.CriticalityScore"/> — clamped to 1–10.
    /// </summary>
    public static int Compute(TaskPriority priority, IReadOnlyCollection<int> voteValues, int criticalityScore)
    {
        var priorityNumeric = PriorityToNumeric(priority);
        var avgUserVote = ComputeAvgUserVote(voteValues, priority);

        var raw = (priorityNumeric + avgUserVote + criticalityScore) / 3.0;
        var score = (int)Math.Ceiling(raw);

        return Math.Clamp(score, 1, 10);
    }
}
