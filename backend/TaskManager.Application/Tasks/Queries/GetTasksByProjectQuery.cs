using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Tasks.Queries;

/// <param name="Archived">true = archived tasks only; false (default) = non-archived only.</param>
/// <param name="CompletedWithinDays">When set, drops tasks completed more than this many days ago.</param>
public record GetTasksByProjectQuery(Guid ProjectId, bool Archived = false, int? CompletedWithinDays = null)
    : IRequest<List<ProjectTaskDto>>;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, List<ProjectTaskDto>>
{
    private readonly IUnitOfWork _uow;

    public GetTasksByProjectQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ProjectTaskDto>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _uow.Tasks.GetByProjectAsync(
            request.ProjectId, request.Archived, request.CompletedWithinDays, cancellationToken);
        return tasks.Select(t => t.ToDto()).ToList();
    }
}
