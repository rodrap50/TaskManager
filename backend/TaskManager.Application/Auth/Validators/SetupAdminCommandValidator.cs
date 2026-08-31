using FluentValidation;
using TaskManager.Application.Auth.Commands;

namespace TaskManager.Application.Auth.Validators;

public class SetupAdminCommandValidator : AbstractValidator<SetupAdminCommand>
{
    public SetupAdminCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username may only contain letters, digits, underscores, or hyphens.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
