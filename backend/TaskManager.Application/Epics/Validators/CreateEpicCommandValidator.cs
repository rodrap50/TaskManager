using FluentValidation;
using TaskManager.Application.Epics.Commands;

namespace TaskManager.Application.Epics.Validators;

public class CreateEpicCommandValidator : AbstractValidator<CreateEpicCommand>
{
    public CreateEpicCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Epic name is required.")
            .MaximumLength(100).WithMessage("Epic name must not exceed 100 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be a non-negative integer.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}
