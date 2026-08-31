using FluentValidation;
using TaskManager.Application.Projects.Commands;

namespace TaskManager.Application.Projects.Validators;

public class SetProjectCriticalityScoreCommandValidator : AbstractValidator<SetProjectCriticalityScoreCommand>
{
    public SetProjectCriticalityScoreCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.CriticalityScore)
            .InclusiveBetween(1, 10).WithMessage("CriticalityScore must be between 1 and 10.");
    }
}
