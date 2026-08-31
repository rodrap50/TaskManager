using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Users.Commands;

public record SetUserAdminCommand(Guid Id, bool IsAdmin) : IRequest<AppUserDto?>;

public class SetUserAdminCommandHandler : IRequestHandler<SetUserAdminCommand, AppUserDto?>
{
    private readonly IUnitOfWork _uow;

    public SetUserAdminCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AppUserDto?> Handle(SetUserAdminCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.Id, cancellationToken);
        if (user is null) return null;

        if (request.IsAdmin)
        {
            // Mirrors the DB-level constraint AUTH02.3 introduced (the "IX_Users_SingleAdmin"
            // partial unique index) — this app allows exactly one admin at a time, never zero,
            // never more than one. Checking here turns what would otherwise be a raw
            // DbUpdateException from the unique-index violation into a clean rejection.
            if (!user.IsAdmin && await _uow.Users.ExistsAdminAsync(cancellationToken))
                throw new InvalidOperationException(
                    "An admin already exists. Revoke the current admin before promoting a new one.");

            user.PromoteToAdmin();
        }
        else
        {
            if (user.IsAdmin && await _uow.Users.CountAdminsAsync(cancellationToken) <= 1)
                throw new InvalidOperationException(
                    "Cannot revoke admin status: at least one admin must remain in the system.");

            user.RevokeAdmin();
        }

        await _uow.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
