using FluentValidation;
using TaskManager.Application.Tasks.Commands;

namespace TaskManager.Application.Tasks.Validators;

public class CastVoteCommandValidator : AbstractValidator<CastVoteCommand>
{
    public CastVoteCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.VoteValue)
            .InclusiveBetween(1, 10).WithMessage("VoteValue must be between 1 and 10.");
    }
}
