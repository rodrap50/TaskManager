using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Tasks.Commands;

public record ArchiveTaskCommand(Guid Id, bool Restore = false) : IRequest<ProjectTaskDto?>;

public class ArchiveTaskCommandHandler : IRequestHandler<ArchiveTaskCommand, ProjectTaskDto?>
{
    private readonly IUnitOfWork _uow;

    public ArchiveTaskCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectTaskDto?> Handle(ArchiveTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _uow.Tasks.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return null;

        if (request.Restore)
            task.Restore();
        else
            task.Archive();

        await _uow.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }
}
