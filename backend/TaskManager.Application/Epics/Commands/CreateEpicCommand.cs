using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Epics.Commands;

public record CreateEpicCommand(
    Guid ProjectId,
    string Name,
    int DisplayOrder,
    string? Description = null,
    string? ColorHex = null) : IRequest<EpicDto?>;

public class CreateEpicCommandHandler : IRequestHandler<CreateEpicCommand, EpicDto?>
{
    private readonly IUnitOfWork _uow;

    public CreateEpicCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EpicDto?> Handle(CreateEpicCommand request, CancellationToken cancellationToken)
    {
        var projectExists = await _uow.Projects.GetByIdAsync(request.ProjectId, cancellationToken);
        if (projectExists is null) return null;

        var epic = new Epic(
            projectId:    request.ProjectId,
            name:         request.Name,
            displayOrder: request.DisplayOrder,
            description:  request.Description,
            colorHex:     request.ColorHex);

        _uow.Epics.Add(epic);
        await _uow.SaveChangesAsync(cancellationToken);

        return epic.ToDto();
    }
}
