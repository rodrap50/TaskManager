using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tasks.Commands;

public record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    TaskPriority Priority = TaskPriority.Medium,
    string? Description = null,
    Guid? PhaseId = null,
    Guid? EpicId = null,
    Guid? AssignedUserId = null,
    DateTime? DueDate = null,
    decimal? EstimatedHours = null) : IRequest<ProjectTaskDto?>;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, ProjectTaskDto?>
{
    private readonly IUnitOfWork _uow;

    public CreateTaskCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectTaskDto?> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var project = await _uow.Projects.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null) return null;

        var task = new ProjectTask(
            projectId:      request.ProjectId,
            title:          request.Title,
            priority:       request.Priority,
            description:    request.Description,
            phaseId:        request.PhaseId,
            assignedUserId: request.AssignedUserId,
            dueDate:        request.DueDate,
            estimatedHours: request.EstimatedHours);

        if (request.EpicId.HasValue)
            task.AssignToEpic(request.EpicId.Value);

        // Recompute now the real project (rather than the constructor's DefaultCriticalityScore
        // placeholder) is known — a no-op for projects still at the default, but correct for
        // ones an admin already re-tuned (PRI01.4).
        task.RecalculateWeightedScore(project.CriticalityScore);

        _uow.Tasks.Add(task);
        await _uow.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }
}
