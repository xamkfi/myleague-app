using Application.Features.Hockey.Matches.Commands;
using Domain.Enums.Hockey.Matches;
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

        RuleFor(x => x.PlayoffMatchOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PlayoffMatchOrder.HasValue);

        RuleFor(x => x)
            .Must(x => x.NextMatchId.HasValue == x.NextMatchSlot.HasValue)
            .WithMessage("NextMatchId and NextMatchSlot must be provided together.");

        RuleFor(x => x.NextMatchSlot)
            .Must(slot => slot is HockeyTeamSlot.Home or HockeyTeamSlot.Away)
            .When(x => x.NextMatchSlot.HasValue)
            .WithMessage("Next match slot must be Home or Away.");

        RuleFor(x => x.PlayoffRound)
            .NotNull()
            .When(x => x.NextMatchId.HasValue)
            .WithMessage("Playoff round is required when a next match is set.");
    }
}
