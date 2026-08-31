namespace TaskManager.Application.Common.Interfaces;

/// <summary>
/// In-memory, thread-safe cache of currently-allowed CORS origins backing the "Default"
/// CORS policy's <c>SetIsOriginAllowed</c> predicate. Command handlers call
/// <see cref="SetOrigins"/> after mutating <see cref="Domain.Entities.AllowedOrigin"/> rows
/// so a change takes effect immediately, with no API restart required.
/// </summary>
public interface IAllowedOriginCache
{
    bool IsAllowed(string origin);
    void SetOrigins(IEnumerable<string> origins);
}
