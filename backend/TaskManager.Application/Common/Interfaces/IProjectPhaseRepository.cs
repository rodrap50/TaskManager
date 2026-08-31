using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common.Interfaces;

public interface IProjectPhaseRepository : IRepository<ProjectPhase>
{
    Task<IReadOnlyList<ProjectPhase>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
}
