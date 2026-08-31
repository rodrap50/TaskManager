namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a non-human/service-account credential (e.g. Home Assistant, an
/// automation script) that can authenticate against the API without a human login.
/// </summary>
/// <remarks>
/// The raw token value is never persisted or reconstructible from this entity — only
/// <see cref="TokenHash"/> (computed by the Application layer before construction, the
/// same pattern <see cref="AppUser.PasswordHash"/> follows) is stored.
/// </remarks>
public sealed class ApiToken
{
    public Guid Id { get; private set; }

    /// <summary>Human-readable label shown in the admin token list (e.g. "Home Assistant").</summary>
    public string Name { get; private set; }

    /// <summary>Pre-computed hash of the raw token. The raw value is never stored.</summary>
    public string TokenHash { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    /// <summary>Non-null once revoked; the token is invalid for auth purposes from that point on.</summary>
    public DateTime? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public ApiToken(string name, string tokenHash, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or whitespace.", nameof(name));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash must not be null or whitespace.", nameof(tokenHash));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId must be a valid, non-empty GUID.", nameof(createdByUserId));

        Id              = Guid.NewGuid();
        Name            = name.Trim();
        TokenHash       = tokenHash;
        CreatedByUserId = createdByUserId;
        CreatedAt       = DateTime.UtcNow;
        RevokedAt       = null;
    }

    /// <summary>
    /// Private parameterless constructor reserved exclusively for EF Core's
    /// proxy/materialization pipeline. Never invoke directly from application code.
    /// </summary>
    private ApiToken()
    {
        Name      = string.Empty;
        TokenHash = string.Empty;
    }

    /// <summary>Idempotent — revoking an already-revoked token leaves its original RevokedAt.</summary>
    public void Revoke() => RevokedAt ??= DateTime.UtcNow;
}
