using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Tasks.Queries;

public record GetTaskByIdQuery(Guid Id) : IRequest<ProjectTaskDto?>;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, ProjectTaskDto?>
{
    private readonly IUnitOfWork _uow;

    public GetTaskByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectTaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _uow.Tasks.GetByIdAsync(request.Id, cancellationToken);
        return task?.ToDto();
    }
}
