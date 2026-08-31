using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common.Interfaces;

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsAdminAsync(CancellationToken ct = default);
    Task<int> CountAdminsAsync(CancellationToken ct = default);

    /// <summary>Users who are members of the given project, via the <c>ProjectMembers</c> join table.</summary>
    Task<IReadOnlyList<AppUser>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>
    /// Attempts to persist <paramref name="adminUser"/>, relying on the DB's single-admin
    /// partial unique index for correctness under concurrent callers. Returns
    /// <see langword="false"/> (instead of throwing) if the insert collides with that
    /// constraint — e.g. another request won the race and created the admin first.
    /// </summary>
    Task<bool> TryCreateFirstAdminAsync(AppUser adminUser, CancellationToken ct = default);
}
