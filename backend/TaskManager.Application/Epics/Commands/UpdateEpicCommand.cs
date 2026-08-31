using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Epics.Commands;

public record UpdateEpicCommand(
    Guid Id,
    string? Name,
    string? Description,
    int? DisplayOrder) : IRequest<EpicDto?>;

public class UpdateEpicCommandHandler : IRequestHandler<UpdateEpicCommand, EpicDto?>
{
    private readonly IUnitOfWork _uow;

    public UpdateEpicCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EpicDto?> Handle(UpdateEpicCommand request, CancellationToken cancellationToken)
    {
        var epic = await _uow.Epics.GetByIdAsync(request.Id, cancellationToken);
        if (epic is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name))
            epic.Rename(request.Name);

        if (request.Description is not null)
            epic.UpdateDescription(request.Description);

        if (request.DisplayOrder.HasValue)
            epic.Reorder(request.DisplayOrder.Value);

        await _uow.SaveChangesAsync(cancellationToken);

        return epic.ToDto();
    }
}
