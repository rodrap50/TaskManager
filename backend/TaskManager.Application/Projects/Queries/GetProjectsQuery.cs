using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Projects.Queries;

public record GetProjectsQuery() : IRequest<List<ProjectDto>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IUnitOfWork _uow;

    public GetProjectsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _uow.Projects.GetAllWithChildrenAsync(cancellationToken);
        return projects.Select(p => p.ToDto()).ToList();
    }
}
