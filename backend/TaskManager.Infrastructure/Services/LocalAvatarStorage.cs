using Microsoft.Extensions.Hosting;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Infrastructure.Services;

/// <summary>
/// Saves avatar images to <c>wwwroot/avatars/</c> on local disk. Served back out via
/// ASP.NET Core's static file middleware (see <c>Program.cs</c>'s <c>UseStaticFiles</c>
/// call), so the returned URL is directly resolvable — no separate download endpoint needed.
/// </summary>
public sealed class LocalAvatarStorage : IAvatarStorage
{
    private readonly string _avatarsDir;

    public LocalAvatarStorage(IHostEnvironment env)
    {
        _avatarsDir = Path.Combine(env.ContentRootPath, "wwwroot", "avatars");
    }

    public async Task<string> SaveAsync(Guid userId, string extension, Stream content, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_avatarsDir);

        // Remove any previously stored avatar for this user, even one saved under a
        // different extension (e.g. re-uploading a .jpg after an earlier .png) — otherwise
        // the stale file lingers on disk, unreferenced, once AvatarUrl moves on.
        foreach (var stale in Directory.EnumerateFiles(_avatarsDir, $"{userId}.*"))
            File.Delete(stale);

        var fileName = $"{userId}{extension}";
        var path     = Path.Combine(_avatarsDir, fileName);

        await using var fileStream = File.Create(path);
        await content.CopyToAsync(fileStream, ct);

        return $"/avatars/{fileName}";
    }
}
