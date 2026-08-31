using TaskManager.Application.Common.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Projects = new ProjectRepository(context);
        Tasks    = new ProjectTaskRepository(context);
        Phases   = new ProjectPhaseRepository(context);
        Epics    = new EpicRepository(context);
        Users    = new UserRepository(context);
        ApiTokens = new ApiTokenRepository(context);
        AllowedOrigins = new AllowedOriginRepository(context);
    }

    public IProjectRepository Projects { get; }
    public IProjectTaskRepository Tasks { get; }
    public IProjectPhaseRepository Phases { get; }
    public IEpicRepository Epics { get; }
    public IUserRepository Users { get; }
    public IApiTokenRepository ApiTokens { get; }
    public IAllowedOriginRepository AllowedOrigins { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
