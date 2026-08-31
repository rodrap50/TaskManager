using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Projects.Members.Commands;

public enum AddProjectMemberOutcome { ProjectNotFound, UserNotFound, Success }

public record AddProjectMemberResult(AddProjectMemberOutcome Outcome, AppUserDto? Member);

public record AddProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest<AddProjectMemberResult>;

public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, AddProjectMemberResult>
{
    private readonly IUnitOfWork _uow;

    public AddProjectMemberCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AddProjectMemberResult> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        var project = await _uow.Projects.GetWithMembersAsync(request.ProjectId, cancellationToken);
        if (project is null)
            return new AddProjectMemberResult(AddProjectMemberOutcome.ProjectNotFound, null);

        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return new AddProjectMemberResult(AddProjectMemberOutcome.UserNotFound, null);

        project.AddMember(request.UserId);
        await _uow.SaveChangesAsync(cancellationToken);

        return new AddProjectMemberResult(AddProjectMemberOutcome.Success, user.ToDto());
    }
}
