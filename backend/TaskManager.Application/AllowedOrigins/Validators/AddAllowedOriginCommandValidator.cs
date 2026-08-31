using FluentValidation;
using TaskManager.Application.AllowedOrigins.Commands;

namespace TaskManager.Application.AllowedOrigins.Validators;

public class AddAllowedOriginCommandValidator : AbstractValidator<AddAllowedOriginCommand>
{
    public AddAllowedOriginCommandValidator()
    {
        RuleFor(x => x.OriginUrl)
            .NotEmpty().WithMessage("Origin URL is required.")
            .MaximumLength(500).WithMessage("Origin URL must not exceed 500 characters.")
            .Must(BeAValidHttpOrigin).WithMessage("Origin URL must be an absolute http:// or https:// URL.");
    }

    private static bool BeAValidHttpOrigin(string originUrl)
    {
        if (string.IsNullOrWhiteSpace(originUrl)) return false;

        return Uri.TryCreate(originUrl.Trim().TrimEnd('/'), UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
