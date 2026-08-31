using MediatR;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Auth.Commands;

/// <summary>
/// Creates the very first admin account on a fresh deployment. Only succeeds while no
/// admin exists yet — see <see cref="IUserRepository.TryCreateFirstAdminAsync"/> for the
/// DB-level guarantee this relies on under concurrent callers.
/// </summary>
public record SetupAdminCommand(string Username, string Password) : IRequest<LoginResult?>;

public class SetupAdminCommandHandler : IRequestHandler<SetupAdminCommand, LoginResult?>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public SetupAdminCommandHandler(IUnitOfWork uow, IPasswordHasher hasher, IJwtTokenGenerator tokenGenerator)
    {
        _uow            = uow;
        _hasher         = hasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResult?> Handle(SetupAdminCommand request, CancellationToken cancellationToken)
    {
        // Fast-path only — the DB's single-admin partial unique index is what actually
        // protects against a concurrent double-submission both passing this check.
        if (await _uow.Users.ExistsAdminAsync(cancellationToken))
            return null;

        var passwordHash = _hasher.Hash(request.Password);
        var user = new AppUser(
            username:     request.Username,
            displayName:  request.Username,
            email:        $"{request.Username.Trim().ToLowerInvariant()}@localhost",
            passwordHash: passwordHash,
            isAdmin:      true);

        var created = await _uow.Users.TryCreateFirstAdminAsync(user, cancellationToken);
        if (!created)
            return null;

        var token = _tokenGenerator.GenerateToken(user);
        return new LoginResult(token, user.ToDto());
    }
}
