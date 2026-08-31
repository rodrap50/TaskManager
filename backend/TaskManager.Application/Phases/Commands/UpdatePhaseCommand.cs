using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Phases.Commands;

public record UpdatePhaseCommand(
    Guid Id,
    string? Name,
    string? Description,
    int? DisplayOrder) : IRequest<ProjectPhaseDto?>;

public class UpdatePhaseCommandHandler : IRequestHandler<UpdatePhaseCommand, ProjectPhaseDto?>
{
    private readonly IUnitOfWork _uow;

    public UpdatePhaseCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectPhaseDto?> Handle(UpdatePhaseCommand request, CancellationToken cancellationToken)
    {
        var phase = await _uow.Phases.GetByIdAsync(request.Id, cancellationToken);
        if (phase is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name))
            phase.Rename(request.Name);

        if (request.Description is not null)
            phase.UpdateDescription(request.Description);

        if (request.DisplayOrder.HasValue)
            phase.Reorder(request.DisplayOrder.Value);

        await _uow.SaveChangesAsync(cancellationToken);

        return phase.ToDto();
    }
}
