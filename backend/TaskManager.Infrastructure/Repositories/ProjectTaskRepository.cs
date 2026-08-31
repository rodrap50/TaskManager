using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class ProjectTaskRepository : GenericRepository<ProjectTask>, IProjectTaskRepository
{
    public ProjectTaskRepository(AppDbContext context) : base(context) { }

    // Overridden (rather than inheriting GenericRepository's FindAsync-based lookup) so
    // Votes is always eagerly loaded — RecalculateWeightedScore reads it in-memory, and a
    // plain FindAsync never populates a collection navigation on a fresh context instance.
    public override async Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Context.Tasks
            .Include(t => t.Votes)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<ProjectTask>> GetByProjectAsync(Guid projectId, CancellationToken ct = default)
        => await Context.Tasks
            .Include(t => t.Votes)
            .Where(t => t.ProjectId == projectId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProjectTask>> GetByAssigneeAsync(Guid userId, CancellationToken ct = default)
        => await Context.Tasks
            .Where(t => t.AssignedUserId == userId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProjectTask>> GetTrackedByProjectAsync(Guid projectId, CancellationToken ct = default)
        => await Context.Tasks
            .Include(t => t.Votes)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync(ct);
}
