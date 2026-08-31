namespace TaskManager.Application.Common.Interfaces;

/// <summary>
/// Persists user avatar images to local storage (data-sovereignty requirement — never a
/// cloud/object-storage provider). Implementations must overwrite any previously stored
/// avatar for the same user, including one saved under a different file extension.
/// </summary>
public interface IAvatarStorage
{
    /// <summary>
    /// Saves <paramref name="content"/> as the avatar for <paramref name="userId"/> and
    /// returns the public, resolvable URL path (e.g. <c>/avatars/{userId}.png</c>).
    /// </summary>
    Task<string> SaveAsync(Guid userId, string extension, Stream content, CancellationToken ct = default);
}
