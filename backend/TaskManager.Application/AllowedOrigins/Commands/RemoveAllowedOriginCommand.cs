using MediatR;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Application.AllowedOrigins.Commands;

public record RemoveAllowedOriginCommand(Guid Id) : IRequest<bool>;

public class RemoveAllowedOriginCommandHandler : IRequestHandler<RemoveAllowedOriginCommand, bool>
{
    private readonly IUnitOfWork _uow;
    private readonly IAllowedOriginCache _cache;

    public RemoveAllowedOriginCommandHandler(IUnitOfWork uow, IAllowedOriginCache cache)
    {
        _uow   = uow;
        _cache = cache;
    }

    public async Task<bool> Handle(RemoveAllowedOriginCommand request, CancellationToken cancellationToken)
    {
        var origin = await _uow.AllowedOrigins.GetByIdAsync(request.Id, cancellationToken);
        if (origin is null) return false;

        _uow.AllowedOrigins.Remove(origin);
        await _uow.SaveChangesAsync(cancellationToken);

        var all = await _uow.AllowedOrigins.GetAllAsync(cancellationToken);
        _cache.SetOrigins(all.Select(o => o.OriginUrl));

        return true;
    }
}
