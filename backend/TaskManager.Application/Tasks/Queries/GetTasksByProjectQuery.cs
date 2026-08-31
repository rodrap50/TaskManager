using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Tasks.Queries;

public record GetTasksByProjectQuery(Guid ProjectId) : IRequest<List<ProjectTaskDto>>;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, List<ProjectTaskDto>>
{
    private readonly IUnitOfWork _uow;

    public GetTasksByProjectQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ProjectTaskDto>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _uow.Tasks.GetByProjectAsync(request.ProjectId, cancellationToken);
        return tasks.Select(t => t.ToDto()).ToList();
    }
}
