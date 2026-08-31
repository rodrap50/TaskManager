using MediatR;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Application.ApiTokens.Commands;

public record RevokeApiTokenCommand(Guid Id) : IRequest<bool>;

public class RevokeApiTokenCommandHandler : IRequestHandler<RevokeApiTokenCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public RevokeApiTokenCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(RevokeApiTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _uow.ApiTokens.GetByIdAsync(request.Id, cancellationToken);
        if (token is null) return false;

        token.Revoke();
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
