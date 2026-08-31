using MediatR;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Application.Projects.Members.Commands;

public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest<bool>;

public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public RemoveProjectMemberCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
    {
        var project = await _uow.Projects.GetWithMembersAsync(request.ProjectId, cancellationToken);
        if (project is null) return false;

        project.RemoveMember(request.UserId);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
