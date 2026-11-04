using FluentValidation;
using TaskManager.Application.Tasks.Queries;

namespace TaskManager.Application.Tasks.Validators;

public class GetTasksByProjectQueryValidator : AbstractValidator<GetTasksByProjectQuery>
{
    public GetTasksByProjectQueryValidator()
    {
        RuleFor(x => x.CompletedWithinDays)
            .InclusiveBetween(0, 3650).When(x => x.CompletedWithinDays.HasValue)
            .WithMessage("CompletedWithinDays must be between 0 and 3650.");
    }
}
