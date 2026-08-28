using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

/// <summary>
/// Validator for <see cref="ImportHockeyMatchEventsCommand"/>.
/// </summary>
public class ImportHockeyMatchEventsCommandValidator : AbstractValidator<ImportHockeyMatchEventsCommand>
{
    public const int MaxEvents = 200;

    public ImportHockeyMatchEventsCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");

        RuleFor(x => x.Events)
            .NotNull().WithMessage("Events are required")
            .Must(events => events.Count > 0).WithMessage("At least one event is required")
            .Must(events => events.Count <= MaxEvents)
            .WithMessage($"At most {MaxEvents} events can be imported in one request");

        RuleForEach(x => x.Events).ChildRules(eventItem =>
        {
            eventItem.RuleFor(e => e.EventType)
                .NotEmpty().WithMessage("Event type is required")
                .Must(type =>
                    string.Equals(type, "Goal", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type, "Penalty", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Event type must be Goal or Penalty");

            eventItem.RuleFor(e => e.MatchTeamId)
                .NotEmpty().WithMessage("Match team ID is required")
                .NotEqual(Guid.Empty).WithMessage("Match team ID cannot be empty");

            eventItem.RuleFor(e => e.PeriodNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Period number must be 1 or greater");

            eventItem.RuleFor(e => e.TimeInSeconds)
                .GreaterThanOrEqualTo(0).WithMessage("Time must be non-negative");

            eventItem.RuleFor(e => e.ActivePlayerId)
                .NotEmpty().WithMessage("Scorer active player ID is required for a goal")
                .When(e => string.Equals(e.EventType, "Goal", StringComparison.OrdinalIgnoreCase));

            eventItem.RuleFor(e => e.PenaltyMinutes)
                .InclusiveBetween(0, 20).WithMessage("Penalty duration must be between 0 and 20 minutes")
                .When(e => string.Equals(e.EventType, "Penalty", StringComparison.OrdinalIgnoreCase));
        });
    }
}
