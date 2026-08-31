using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class UserRepository : GenericRepository<AppUser>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await Context.Users
            .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant(), ct);

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        => await Context.Users
            .AnyAsync(u => u.Username == username.ToLowerInvariant(), ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await Context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<bool> ExistsAdminAsync(CancellationToken ct = default)
        => await Context.Users
            .AnyAsync(u => u.IsAdmin, ct);

    public async Task<int> CountAdminsAsync(CancellationToken ct = default)
        => await Context.Users
            .CountAsync(u => u.IsAdmin, ct);

    public async Task<IReadOnlyList<AppUser>> GetByProjectAsync(Guid projectId, CancellationToken ct = default)
        => await Context.Users
            .Join(Context.ProjectMembers.Where(m => m.ProjectId == projectId),
                  u => u.Id, m => m.UserId, (u, _) => u)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<bool> TryCreateFirstAdminAsync(AppUser adminUser, CancellationToken ct = default)
    {
        Context.Users.Add(adminUser);
        try
        {
            await Context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            // Single-admin partial unique index (IX_Users_SingleAdmin) rejected a concurrent
            // insert — someone else's setup request won the race.
            Context.Entry(adminUser).State = EntityState.Detached;
            return false;
        }
    }
}
