using FluentValidation;
using TaskManager.Application.Projects.Commands;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Projects.Validators;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

        RuleFor(x => x.Scope)
            .IsInEnum().WithMessage("Invalid project scope.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ColorHex)
            .Matches(@"^#?[0-9A-Fa-f]{6}$").WithMessage("ColorHex must be a valid 6-character hex color.")
            .When(x => x.ColorHex is not null);
    }
}
