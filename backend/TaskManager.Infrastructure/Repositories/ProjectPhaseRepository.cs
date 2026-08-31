using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class ProjectPhaseRepository : GenericRepository<ProjectPhase>, IProjectPhaseRepository
{
    public ProjectPhaseRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ProjectPhase>> GetByProjectAsync(Guid projectId, CancellationToken ct = default)
        => await Context.Phases
            .Where(ph => ph.ProjectId == projectId)
            .OrderBy(ph => ph.DisplayOrder)
            .AsNoTracking()
            .ToListAsync(ct);
}
