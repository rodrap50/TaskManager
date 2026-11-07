using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Services;

namespace TaskManager.Application.Projects.Commands;

public record CreateProjectCommand(
    string Name,
    ProjectScope Scope,
    string? Description = null,
    string? ColorHex = null,
    DateTime? DueDate = null) : IRequest<ProjectDto>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateProjectCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUser.UserId;

        // ponytail: two creates racing for the same new key fail one on the unique index (500); retry on conflict if that ever shows up.
        var key = ProjectKeys.PickUnique(request.Name, await _uow.Projects.GetKeysAsync(cancellationToken));

        var project = new Project(
            name: request.Name,
            scope: request.Scope,
            createdByUserId: ownerId,
            key: key,
            description: request.Description,
            colorHex: request.ColorHex,
            dueDate: request.DueDate);

        project.AddMember(ownerId);

        _uow.Projects.Add(project);
        await _uow.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }
}
