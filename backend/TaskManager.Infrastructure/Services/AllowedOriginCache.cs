using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Infrastructure.Services;

public sealed class AllowedOriginCache : IAllowedOriginCache
{
    private readonly object _lock = new();
    private HashSet<string> _origins = new(StringComparer.OrdinalIgnoreCase);

    public bool IsAllowed(string origin)
    {
        lock (_lock)
        {
            return _origins.Contains(origin);
        }
    }

    public void SetOrigins(IEnumerable<string> origins)
    {
        var next = new HashSet<string>(origins, StringComparer.OrdinalIgnoreCase);
        lock (_lock)
        {
            _origins = next;
        }
    }
}
