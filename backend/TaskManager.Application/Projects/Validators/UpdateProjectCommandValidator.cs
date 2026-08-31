using FluentValidation;
using TaskManager.Application.Projects.Commands;

namespace TaskManager.Application.Projects.Validators;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name must not be empty when provided.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Scope)
            .IsInEnum().WithMessage("Invalid project scope.")
            .When(x => x.Scope.HasValue);

        RuleFor(x => x.ColorHex)
            .Matches(@"^#?[0-9A-Fa-f]{6}$").WithMessage("ColorHex must be a valid 6-character hex color.")
            .When(x => x.ColorHex is not null);
    }
}
