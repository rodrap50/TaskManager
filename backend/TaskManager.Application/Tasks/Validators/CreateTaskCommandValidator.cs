using FluentValidation;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Tasks.Commands;

namespace TaskManager.Application.Tasks.Validators;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator(IUnitOfWork uow)
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(300).WithMessage("Task title must not exceed 300 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority.");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0).WithMessage("EstimatedHours must be a positive value.")
            .When(x => x.EstimatedHours.HasValue);

        RuleFor(x => x.AssignedUserId)
            .MustAsync(async (cmd, userId, ct) => !userId.HasValue || await uow.Projects.IsMemberAsync(cmd.ProjectId, userId.Value, ct))
            .WithMessage("Assignee must be a member of the project.");
    }
}
