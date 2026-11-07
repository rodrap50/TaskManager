using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Services;

namespace TaskManager.Application.Tasks.Queries;

/// <summary>Looks a task up by its display ID ("TSKM-001", any case). Null when the ID is malformed or unknown.</summary>
public record GetTaskByDisplayIdQuery(string DisplayId) : IRequest<ProjectTaskDto?>;

public class GetTaskByDisplayIdQueryHandler : IRequestHandler<GetTaskByDisplayIdQuery, ProjectTaskDto?>
{
    private readonly IUnitOfWork _uow;

    public GetTaskByDisplayIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectTaskDto?> Handle(GetTaskByDisplayIdQuery request, CancellationToken cancellationToken)
    {
        if (!ProjectKeys.TryParseDisplayId(request.DisplayId, out var key, out var number))
            return null;

        var task = await _uow.Tasks.GetByDisplayIdAsync(key, number, cancellationToken);
        return task?.ToDto();
    }
}
