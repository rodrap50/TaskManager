using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.ApiTokens.Queries;

public record GetApiTokensQuery : IRequest<List<ApiTokenDto>>;

public class GetApiTokensQueryHandler : IRequestHandler<GetApiTokensQuery, List<ApiTokenDto>>
{
    private readonly IUnitOfWork _uow;

    public GetApiTokensQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ApiTokenDto>> Handle(GetApiTokensQuery request, CancellationToken cancellationToken)
    {
        var tokens = await _uow.ApiTokens.GetAllAsync(cancellationToken);
        return tokens
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.ToDto())
            .ToList();
    }
}
