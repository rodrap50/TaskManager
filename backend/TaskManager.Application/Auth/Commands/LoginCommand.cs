using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Auth.Commands;

public record LoginCommand(string Username, string Password) : IRequest<LoginResult?>;

public record LoginResult(string Token, AppUserDto User);

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult?>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCommandHandler(IUnitOfWork uow, IPasswordHasher hasher, IJwtTokenGenerator tokenGenerator)
    {
        _uow             = uow;
        _hasher          = hasher;
        _tokenGenerator  = tokenGenerator;
    }

    public async Task<LoginResult?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !user.IsActive)
            return null;

        bool passwordValid;
        try
        {
            passwordValid = _hasher.Verify(request.Password, user.PasswordHash);
        }
        catch
        {
            // Non-bcrypt hashes (e.g. the seeded system placeholder account) fail to
            // parse rather than mismatch — treat that the same as a wrong password.
            passwordValid = false;
        }

        if (!passwordValid)
            return null;

        var token = _tokenGenerator.GenerateToken(user);
        return new LoginResult(token, user.ToDto());
    }
}
