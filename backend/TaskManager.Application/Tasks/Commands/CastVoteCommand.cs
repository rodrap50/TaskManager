using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Tasks.Commands;

public record CastVoteCommand(Guid TaskId, Guid UserId, int VoteValue) : IRequest<ProjectTaskDto?>;

public class CastVoteCommandHandler : IRequestHandler<CastVoteCommand, ProjectTaskDto?>
{
    private readonly IUnitOfWork _uow;

    public CastVoteCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectTaskDto?> Handle(CastVoteCommand request, CancellationToken cancellationToken)
    {
        var task = await _uow.Tasks.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null) return null;

        if (!await _uow.Projects.IsMemberAsync(task.ProjectId, request.UserId, cancellationToken))
            throw new ForbiddenAccessException("You must be a member of this task's project to vote on it.");

        task.CastVote(request.UserId, request.VoteValue);

        var project = await _uow.Projects.GetByIdAsync(task.ProjectId, cancellationToken);
        task.RecalculateWeightedScore(project?.CriticalityScore ?? Project.DefaultCriticalityScore);

        await _uow.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }
}
