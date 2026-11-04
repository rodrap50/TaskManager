using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Tasks.Commands;

/// <summary>
/// Permanently deletes a task; its votes go with it (FK cascade). Returns the deleted task so the
/// caller can record what was removed, or <see langword="null"/> when it doesn't exist.
/// Admin-only — enforced at the endpoint, not here.
/// </summary>
public record DeleteTaskCommand(Guid Id) : IRequest<ProjectTaskDto?>;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, ProjectTaskDto?>
{
    private readonly IUnitOfWork _uow;

    public DeleteTaskCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectTaskDto?> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _uow.Tasks.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return null;

        var deleted = task.ToDto();
        _uow.Tasks.Remove(task);
        await _uow.SaveChangesAsync(cancellationToken);
        return deleted;
    }
}
