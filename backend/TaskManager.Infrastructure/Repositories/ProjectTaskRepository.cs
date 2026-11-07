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
    // Project is loaded for the task's display ID.
    public override async Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Context.Tasks
            .Include(t => t.Votes)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<ProjectTask?> GetByDisplayIdAsync(string projectKey, int number, CancellationToken ct = default)
        => await Context.Tasks
            .Include(t => t.Votes)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Project!.Key == projectKey && t.Number == number, ct);

    public async Task<IReadOnlyList<ProjectTask>> GetByProjectAsync(
        Guid projectId, bool archived = false, int? completedWithinDays = null, CancellationToken ct = default)
    {
        var query = Context.Tasks
            .Include(t => t.Votes)
            .Include(t => t.Project)
            .Where(t => t.ProjectId == projectId && t.IsArchived == archived);

        if (completedWithinDays is { } days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);
            query = query.Where(t => t.CompletedAt == null || t.CompletedAt >= cutoff);
        }

        return await query.AsNoTracking().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetCompletedBeforeAsync(
        Guid? projectId, DateTime cutoff, CancellationToken ct = default)
        => await Context.Tasks
            .Where(t => !t.IsArchived && t.CompletedAt != null && t.CompletedAt < cutoff)
            .Where(t => projectId == null || t.ProjectId == projectId)
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
