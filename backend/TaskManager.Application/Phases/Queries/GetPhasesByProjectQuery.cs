using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Phases.Queries;

public record GetPhasesByProjectQuery(Guid ProjectId) : IRequest<List<ProjectPhaseDto>>;

public class GetPhasesByProjectQueryHandler : IRequestHandler<GetPhasesByProjectQuery, List<ProjectPhaseDto>>
{
    private readonly IUnitOfWork _uow;

    public GetPhasesByProjectQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ProjectPhaseDto>> Handle(GetPhasesByProjectQuery request, CancellationToken cancellationToken)
    {
        var phases = await _uow.Phases.GetByProjectAsync(request.ProjectId, cancellationToken);
        return phases.Select(ph => ph.ToDto()).ToList();
    }
}
