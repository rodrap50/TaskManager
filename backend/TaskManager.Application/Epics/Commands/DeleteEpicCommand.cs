using MediatR;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Application.Epics.Commands;

public record DeleteEpicCommand(Guid Id) : IRequest<bool>;

public class DeleteEpicCommandHandler : IRequestHandler<DeleteEpicCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteEpicCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(DeleteEpicCommand request, CancellationToken cancellationToken)
    {
        var epic = await _uow.Epics.GetByIdAsync(request.Id, cancellationToken);
        if (epic is null) return false;

        _uow.Epics.Remove(epic);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
