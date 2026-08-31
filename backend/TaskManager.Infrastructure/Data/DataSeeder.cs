using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data;

public static class DataSeeder
{
    // Legacy placeholder account from before auth existed. No command references this id
    // anymore as of PM01.2 — kept only so existing dev/prod databases don't lose the row.
    private static readonly Guid SystemUserId = new("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(AppDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(u => u.Id == SystemUserId, cancellationToken))
            return;

        logger.LogInformation("Seeding system placeholder user (ID: {Id})", SystemUserId);

        var systemUser = new AppUser(
            username:     "system",
            displayName:  "System",
            email:        "system@localhost",
            passwordHash: "SYSTEM_ACCOUNT_NO_LOGIN",
            isAdmin:      false);

        context.Users.Add(systemUser);

        // Override the auto-generated Id with the well-known placeholder value.
        context.Entry(systemUser).Property("Id").CurrentValue = SystemUserId;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("System placeholder user seeded successfully");
    }

    // Dev-only default so a fresh dev DB isn't broken by CORS becoming DB-backed
    // (PREP.1) — ordinary, admin-editable seed data, not a special-cased bypass.
    private const string DevAllowedOrigin = "http://localhost:5173";

    public static async Task SeedDevelopmentDataAsync(AppDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await context.AllowedOrigins.AnyAsync(cancellationToken))
            return;

        logger.LogInformation("Seeding default dev AllowedOrigin ({Origin})", DevAllowedOrigin);

        context.AllowedOrigins.Add(new AllowedOrigin(DevAllowedOrigin));

        await context.SaveChangesAsync(cancellationToken);
    }
}
