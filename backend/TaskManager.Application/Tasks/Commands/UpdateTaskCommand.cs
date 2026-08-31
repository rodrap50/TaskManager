using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tasks.Commands;

public record UpdateTaskCommand(
    Guid Id,
    string? Title,
    string? Description,
    TaskPriority? Priority,
    Guid? PhaseId,
    bool ClearPhase,
    Guid? EpicId,
    bool ClearEpic,
    Guid? AssignedUserId,
    bool ClearAssignee,
    Guid? SecondaryAssigneeId,
    bool ClearSecondaryAssignee,
    DateTime? DueDate,
    decimal? EstimatedHours) : IRequest<ProjectTaskDto?>;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, ProjectTaskDto?>
{
    private readonly IUnitOfWork _uow;

    public UpdateTaskCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectTaskDto?> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _uow.Tasks.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Title))
            task.UpdateTitle(request.Title);

        if (request.Description is not null)
            task.UpdateDescription(request.Description);

        if (request.Priority.HasValue)
        {
            task.Reprioritize(request.Priority.Value);

            var project = await _uow.Projects.GetByIdAsync(task.ProjectId, cancellationToken);
            task.RecalculateWeightedScore(project?.CriticalityScore ?? Project.DefaultCriticalityScore);
        }

        if (request.ClearPhase)
            task.RemoveFromPhase();
        else if (request.PhaseId.HasValue)
            task.AssignToPhase(request.PhaseId.Value);

        if (request.ClearEpic)
            task.RemoveFromEpic();
        else if (request.EpicId.HasValue)
            task.AssignToEpic(request.EpicId.Value);

        if (request.ClearAssignee)
            task.Unassign();
        else if (request.AssignedUserId.HasValue)
            task.AssignTo(request.AssignedUserId.Value);

        if (request.ClearSecondaryAssignee)
            task.ClearSecondaryAssignee();
        else if (request.SecondaryAssigneeId.HasValue)
            task.SetSecondaryAssignee(request.SecondaryAssigneeId.Value);

        if (request.DueDate is not null)
            task.SetDueDate(request.DueDate);

        if (request.EstimatedHours is not null)
            task.SetEstimatedHours(request.EstimatedHours);

        await _uow.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }
}
