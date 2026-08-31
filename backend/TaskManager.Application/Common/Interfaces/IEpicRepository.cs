using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common.Interfaces;

public interface IEpicRepository : IRepository<Epic>
{
    Task<IReadOnlyList<Epic>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
}
