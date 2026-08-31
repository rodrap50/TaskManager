namespace TaskManager.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IProjectRepository Projects { get; }
    IProjectTaskRepository Tasks { get; }
    IProjectPhaseRepository Phases { get; }
    IEpicRepository Epics { get; }
    IUserRepository Users { get; }
    IApiTokenRepository ApiTokens { get; }
    IAllowedOriginRepository AllowedOrigins { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
