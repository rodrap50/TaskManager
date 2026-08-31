using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Projects.Commands;

public record UpdateProjectCommand(
    Guid Id,
    string? Name,
    string? Description,
    ProjectScope? Scope,
    string? ColorHex,
    DateTime? DueDate) : IRequest<ProjectDto?>;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto?>
{
    private readonly IUnitOfWork _uow;

    public UpdateProjectCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectDto?> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _uow.Projects.GetWithChildrenAsync(request.Id, cancellationToken);
        if (project is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name))
            project.Rename(request.Name);

        if (request.Description is not null)
            project.UpdateDescription(request.Description);

        if (request.Scope.HasValue)
            project.ChangeScope(request.Scope.Value);

        if (request.ColorHex is not null)
            project.SetColor(request.ColorHex);

        if (request.DueDate is not null)
            project.SetDueDate(request.DueDate);

        await _uow.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }
}
