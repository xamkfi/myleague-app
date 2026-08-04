using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class CreateHockeyMatchCommandValidator : AbstractValidator<CreateHockeyMatchCommand>
{
    public CreateHockeyMatchCommandValidator()
    {
        RuleFor(x => x.ScheduledStartTime)
            .NotEqual(default(DateTime)).WithMessage("Scheduled start time is required.");

        RuleFor(x => x.MatchType)
            .IsInEnum().WithMessage("Match type is required.");

        RuleFor(x => x.Venue)
            .MaximumLength(200).When(x => x.Venue is not null);
    }
}
