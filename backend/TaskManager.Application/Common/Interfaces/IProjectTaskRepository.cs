using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common.Interfaces;

public interface IProjectTaskRepository : IRepository<ProjectTask>
{
    /// <summary>
    /// Tasks in a project. <paramref name="archived"/> selects archived tasks only, or (default)
    /// non-archived only. <paramref name="completedWithinDays"/>, when set, drops completed tasks
    /// whose <see cref="ProjectTask.CompletedAt"/> is older than that many days; open tasks are kept.
    /// </summary>
    Task<IReadOnlyList<ProjectTask>> GetByProjectAsync(
        Guid projectId, bool archived = false, int? completedWithinDays = null, CancellationToken ct = default);

    /// <summary>
    /// Change-tracked, non-archived, completed tasks whose <see cref="ProjectTask.CompletedAt"/>
    /// is before <paramref name="cutoff"/> — the candidates for bulk/automatic archiving.
    /// <paramref name="projectId"/> <see langword="null"/> means every project.
    /// </summary>
    Task<IReadOnlyList<ProjectTask>> GetCompletedBeforeAsync(Guid? projectId, DateTime cutoff, CancellationToken ct = default);

    /// <summary>
    /// The task with display ID <c>{projectKey}-{number}</c>, or <see langword="null"/>.
    /// <paramref name="projectKey"/> must already be uppercase.
    /// </summary>
    Task<ProjectTask?> GetByDisplayIdAsync(string projectKey, int number, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectTask>> GetByAssigneeAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Loads every task in a project as change-tracked entities with <see cref="ProjectTask.Votes"/>
    /// eagerly loaded, for bulk mutations that need to call
    /// <see cref="ProjectTask.RecalculateWeightedScore"/> on each one and persist the result
    /// (e.g. PRI01.4's project-wide recalculation when <c>CriticalityScore</c> changes).
    /// Unlike <see cref="GetByProjectAsync"/>, this is intentionally NOT <c>AsNoTracking()</c>.
    /// </summary>
    Task<IReadOnlyList<ProjectTask>> GetTrackedByProjectAsync(Guid projectId, CancellationToken ct = default);
}
