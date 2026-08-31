using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.AllowedOrigins.Commands;

public record AddAllowedOriginCommand(string OriginUrl) : IRequest<AllowedOriginDto>;

public class AddAllowedOriginCommandHandler : IRequestHandler<AddAllowedOriginCommand, AllowedOriginDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IAllowedOriginCache _cache;

    public AddAllowedOriginCommandHandler(IUnitOfWork uow, IAllowedOriginCache cache)
    {
        _uow   = uow;
        _cache = cache;
    }

    public async Task<AllowedOriginDto> Handle(AddAllowedOriginCommand request, CancellationToken cancellationToken)
    {
        var origin = new AllowedOrigin(request.OriginUrl);

        _uow.AllowedOrigins.Add(origin);
        await _uow.SaveChangesAsync(cancellationToken);

        var all = await _uow.AllowedOrigins.GetAllAsync(cancellationToken);
        _cache.SetOrigins(all.Select(o => o.OriginUrl));

        return origin.ToDto();
    }
}
