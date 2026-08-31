using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Projects.Commands;

public record ArchiveProjectCommand(Guid Id, bool Restore = false) : IRequest<ProjectDto?>;

public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, ProjectDto?>
{
    private readonly IUnitOfWork _uow;

    public ArchiveProjectCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectDto?> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _uow.Projects.GetWithChildrenAsync(request.Id, cancellationToken);
        if (project is null) return null;

        if (request.Restore)
            project.Restore();
        else
            project.Archive();

        await _uow.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }
}
