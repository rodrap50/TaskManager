using FluentValidation;
using TaskManager.Application.ApiTokens.Commands;

namespace TaskManager.Application.ApiTokens.Validators;

public class CreateApiTokenCommandValidator : AbstractValidator<CreateApiTokenCommand>
{
    public CreateApiTokenCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("CreatedByUserId is required.");

        // FixedWindowRateLimiterOptions.PermitLimit (MCP02.4) throws for a non-positive
        // value — reject it here instead of crashing on the token's first request.
        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("RateLimitPerMinute must be a positive integer.")
            .When(x => x.RateLimitPerMinute.HasValue);
    }
}
