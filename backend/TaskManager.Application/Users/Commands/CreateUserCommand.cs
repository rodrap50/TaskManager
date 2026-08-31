using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Users.Commands;

public record CreateUserCommand(
    string Username,
    string DisplayName,
    string Email,
    string Password,
    bool IsAdmin = false) : IRequest<AppUserDto>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, AppUserDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public CreateUserCommandHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow    = uow;
        _hasher = hasher;
    }

    public async Task<AppUserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var passwordHash = _hasher.Hash(request.Password);

        var user = new AppUser(
            username:     request.Username,
            displayName:  request.DisplayName,
            email:        request.Email,
            passwordHash: passwordHash,
            isAdmin:      request.IsAdmin);

        _uow.Users.Add(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
