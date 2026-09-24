using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.API.Authentication;

/// <summary>
/// Authenticates a request via a raw <see cref="ApiToken"/> value carried in the
/// <see cref="ApiTokenAuthenticationDefaults.HeaderName"/> header, as an alternative to a JWT.
/// </summary>
public class ApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public ApiTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUnitOfWork uow,
        IPasswordHasher hasher)
        : base(options, logger, encoder)
    {
        _uow    = uow;
        _hasher = hasher;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiTokenAuthenticationDefaults.HeaderName, out var headerValues))
            return AuthenticateResult.NoResult();

        var rawToken = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(rawToken))
            return AuthenticateResult.NoResult();

        var tokens = await _uow.ApiTokens.GetAllAsync();

        // Bcrypt hashes are salted, so a token row can't be looked up by hash equality —
        // every active token has to be checked. Expected token counts are small
        // (service-account scale, admin-provisioned), so this O(n) scan is an accepted
        // tradeoff rather than something worth indexing around.
        ApiToken? matched = null;
        foreach (var candidate in tokens)
        {
            // Expired is treated exactly like revoked/unknown (MCP02.2) — a match against
            // an expired token never surfaces, so it falls through to the same generic
            // failure below rather than a distinct "expired" message.
            if (candidate.IsRevoked || candidate.IsExpired) continue;

            bool matches;
            try
            {
                matches = _hasher.Verify(rawToken, candidate.TokenHash);
            }
            catch
            {
                matches = false;
            }

            if (matches)
            {
                matched = candidate;
                break;
            }
        }

        if (matched is null)
            return AuthenticateResult.Fail("Invalid, revoked, or expired API token.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, matched.CreatedByUserId.ToString()),
            new("apiTokenId", matched.Id.ToString()),
            new("apiTokenName", matched.Name),
            new("isReadOnly", matched.IsReadOnly ? "true" : "false"),
        };

        if (matched.RateLimitPerMinute is { } rateLimitPerMinute)
            claims.Add(new Claim("rateLimitPerMinute", rateLimitPerMinute.ToString()));

        var identity  = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
