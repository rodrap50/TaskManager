using FluentValidation;
using TaskManager.Application.Tasks.Commands;

namespace TaskManager.Application.Tasks.Validators;

public class ArchiveCompletedTasksCommandValidator : AbstractValidator<ArchiveCompletedTasksCommand>
{
    public ArchiveCompletedTasksCommandValidator()
    {
        RuleFor(x => x.OlderThanDays)
            .InclusiveBetween(0, 3650).WithMessage("OlderThanDays must be between 0 and 3650.");
    }
}
