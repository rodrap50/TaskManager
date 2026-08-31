using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class EpicRepository : GenericRepository<Epic>, IEpicRepository
{
    public EpicRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Epic>> GetByProjectAsync(Guid projectId, CancellationToken ct = default)
        => await Context.Epics
            .Where(e => e.ProjectId == projectId)
            .OrderBy(e => e.DisplayOrder)
            .AsNoTracking()
            .ToListAsync(ct);
}
