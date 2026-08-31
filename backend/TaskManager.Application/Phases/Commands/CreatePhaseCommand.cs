using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Phases.Commands;

public record CreatePhaseCommand(
    Guid ProjectId,
    string Name,
    int DisplayOrder,
    string? Description = null) : IRequest<ProjectPhaseDto?>;

public class CreatePhaseCommandHandler : IRequestHandler<CreatePhaseCommand, ProjectPhaseDto?>
{
    private readonly IUnitOfWork _uow;

    public CreatePhaseCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectPhaseDto?> Handle(CreatePhaseCommand request, CancellationToken cancellationToken)
    {
        var projectExists = await _uow.Projects.GetByIdAsync(request.ProjectId, cancellationToken);
        if (projectExists is null) return null;

        var phase = new ProjectPhase(
            projectId:    request.ProjectId,
            name:         request.Name,
            displayOrder: request.DisplayOrder,
            description:  request.Description);

        _uow.Phases.Add(phase);
        await _uow.SaveChangesAsync(cancellationToken);

        return phase.ToDto();
    }
}
