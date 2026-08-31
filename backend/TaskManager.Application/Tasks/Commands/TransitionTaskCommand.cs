using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Tasks.Commands;

public record TransitionTaskCommand(Guid Id, TaskStatus NewStatus) : IRequest<ProjectTaskDto?>;

public class TransitionTaskCommandHandler : IRequestHandler<TransitionTaskCommand, ProjectTaskDto?>
{
    private readonly IUnitOfWork _uow;

    public TransitionTaskCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectTaskDto?> Handle(TransitionTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _uow.Tasks.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return null;

        task.Transition(request.NewStatus);
        await _uow.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }
}
