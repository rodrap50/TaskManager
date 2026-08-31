using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Epics.Queries;

public record GetEpicsByProjectQuery(Guid ProjectId) : IRequest<List<EpicDto>>;

public class GetEpicsByProjectQueryHandler : IRequestHandler<GetEpicsByProjectQuery, List<EpicDto>>
{
    private readonly IUnitOfWork _uow;

    public GetEpicsByProjectQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<EpicDto>> Handle(GetEpicsByProjectQuery request, CancellationToken cancellationToken)
    {
        var epics = await _uow.Epics.GetByProjectAsync(request.ProjectId, cancellationToken);
        return epics.Select(e => e.ToDto()).ToList();
    }
}
