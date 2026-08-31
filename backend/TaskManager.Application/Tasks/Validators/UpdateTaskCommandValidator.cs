using FluentValidation;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Tasks.Commands;

namespace TaskManager.Application.Tasks.Validators;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator(IUnitOfWork uow)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title must not be empty when provided.")
            .MaximumLength(300).WithMessage("Task title must not exceed 300 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority.")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0).WithMessage("EstimatedHours must be a positive value.")
            .When(x => x.EstimatedHours.HasValue);

        // Assignee/secondary-assignee are only checked against the project roster when the
        // requested value actually differs from the task's current one — the frontend resends
        // the current assignee on every save regardless of whether it changed, so rejecting an
        // unchanged value here would make a task un-editable the moment its assignee is later
        // removed from the project.
        RuleFor(x => x).CustomAsync(async (cmd, context, ct) =>
        {
            if (!cmd.AssignedUserId.HasValue && !cmd.SecondaryAssigneeId.HasValue) return;

            var task = await uow.Tasks.GetByIdAsync(cmd.Id, ct);
            if (task is null) return; // let the handler's null-check 404 this instead

            if (cmd.AssignedUserId.HasValue
                && cmd.AssignedUserId != task.AssignedUserId
                && !await uow.Projects.IsMemberAsync(task.ProjectId, cmd.AssignedUserId.Value, ct))
            {
                context.AddFailure("AssignedUserId", "Assignee must be a member of the task's project.");
            }

            if (cmd.SecondaryAssigneeId.HasValue
                && cmd.SecondaryAssigneeId != task.SecondaryAssigneeId
                && !await uow.Projects.IsMemberAsync(task.ProjectId, cmd.SecondaryAssigneeId.Value, ct))
            {
                context.AddFailure("SecondaryAssigneeId", "Secondary assignee must be a member of the task's project.");
            }
        });
    }
}
