using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<Project?> GetWithChildrenAsync(Guid id, CancellationToken ct = default)
        => await Context.Projects
            .Include(p => p.Phases)
            .Include(p => p.Epics)
            .Include(p => p.Tasks).ThenInclude(t => t.Votes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Project>> GetAllWithChildrenAsync(CancellationToken ct = default)
        => await Context.Projects
            .Include(p => p.Phases)
            .Include(p => p.Epics)
            .Include(p => p.Tasks).ThenInclude(t => t.Votes)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Project?> GetWithMembersAsync(Guid id, CancellationToken ct = default)
        => await Context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> IsMemberAsync(Guid projectId, Guid userId, CancellationToken ct = default)
        => await Context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);
}
