using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common.Interfaces;

public interface IProjectTaskRepository : IRepository<ProjectTask>
{
    Task<IReadOnlyList<ProjectTask>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
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
