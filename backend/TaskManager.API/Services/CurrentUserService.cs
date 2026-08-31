using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.API.Services;

/// <summary>
/// Reads the current authenticated user's id from the request's claims principal.
/// Checks both <see cref="JwtRegisteredClaimNames.Sub"/> (set by JWT login) and
/// <see cref="ClaimTypes.NameIdentifier"/> (set by the <c>ApiToken</c> header scheme) —
/// both auth schemes are live at once via the "SmartAuth" policy scheme in Program.cs.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var sub = user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (sub is null || !Guid.TryParse(sub, out var userId))
                throw new InvalidOperationException("No authenticated user id claim found on the current request.");

            return userId;
        }
    }
}
