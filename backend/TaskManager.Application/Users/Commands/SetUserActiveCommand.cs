using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Users.Commands;

public record SetUserActiveCommand(Guid Id, bool IsActive) : IRequest<AppUserDto?>;

public class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, AppUserDto?>
{
    private readonly IUnitOfWork _uow;

    public SetUserActiveCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AppUserDto?> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.Id, cancellationToken);
        if (user is null) return null;

        if (request.IsActive) user.Reactivate();
        else user.Deactivate();

        await _uow.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
