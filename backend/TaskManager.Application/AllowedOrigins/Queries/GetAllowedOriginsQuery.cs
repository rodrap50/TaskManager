using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.AllowedOrigins.Queries;

public record GetAllowedOriginsQuery : IRequest<List<AllowedOriginDto>>;

public class GetAllowedOriginsQueryHandler : IRequestHandler<GetAllowedOriginsQuery, List<AllowedOriginDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllowedOriginsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<AllowedOriginDto>> Handle(GetAllowedOriginsQuery request, CancellationToken cancellationToken)
    {
        var origins = await _uow.AllowedOrigins.GetAllAsync(cancellationToken);
        return origins
            .OrderBy(o => o.CreatedAt)
            .Select(o => o.ToDto())
            .ToList();
    }
}
