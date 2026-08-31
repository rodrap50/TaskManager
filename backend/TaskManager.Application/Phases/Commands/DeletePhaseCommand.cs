using MediatR;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Application.Phases.Commands;

public record DeletePhaseCommand(Guid Id) : IRequest<bool>;

public class DeletePhaseCommandHandler : IRequestHandler<DeletePhaseCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeletePhaseCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(DeletePhaseCommand request, CancellationToken cancellationToken)
    {
        var phase = await _uow.Phases.GetByIdAsync(request.Id, cancellationToken);
        if (phase is null) return false;

        _uow.Phases.Remove(phase);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
