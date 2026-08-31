namespace TaskManager.Domain.Entities;

/// <summary>
/// A browser origin permitted to make cross-origin requests against the API (CORS).
/// Admin-managed and DB-backed so origins can be added/removed at runtime with no
/// redeploy. Only relevant to browser callers — server-to-server callers (webhooks,
/// service accounts) are never subject to CORS and are gated by <see cref="ApiToken"/>
/// auth instead.
/// </summary>
public sealed class AllowedOrigin
{
    public Guid Id { get; private set; }

    /// <summary>Normalized (no trailing slash) absolute http/https origin, e.g. "https://taskmanager.example.com".</summary>
    public string OriginUrl { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public AllowedOrigin(string originUrl)
    {
        if (string.IsNullOrWhiteSpace(originUrl))
            throw new ArgumentException("Origin URL must not be null or whitespace.", nameof(originUrl));

        var normalized = originUrl.Trim().TrimEnd('/');

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Origin URL must be an absolute http:// or https:// URL.", nameof(originUrl));
        }

        Id        = Guid.NewGuid();
        OriginUrl = normalized;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private AllowedOrigin()
    {
        OriginUrl = string.Empty;
    }
}
