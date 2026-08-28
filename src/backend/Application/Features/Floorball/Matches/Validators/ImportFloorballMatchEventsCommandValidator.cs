using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validator for <see cref="ImportFloorballMatchEventsCommand"/>.
/// </summary>
public class ImportFloorballMatchEventsCommandValidator : AbstractValidator<ImportFloorballMatchEventsCommand>
{
    public const int MaxEvents = 200;

    public ImportFloorballMatchEventsCommandValidator()
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

            eventItem.RuleFor(e => e.TeamId)
                .NotEmpty().WithMessage("Team ID is required")
                .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");

            eventItem.RuleFor(e => e.PeriodNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Period number must be 1 or greater");

            eventItem.RuleFor(e => e.TimeInSeconds)
                .GreaterThanOrEqualTo(0).WithMessage("Time must be non-negative");

            eventItem.RuleFor(e => e.PlayerId)
                .NotEmpty().WithMessage("Player ID is required for a goal")
                .When(e => string.Equals(e.EventType, "Goal", StringComparison.OrdinalIgnoreCase));

            eventItem.RuleFor(e => e.PlayerId)
                .NotEmpty().WithMessage("Player ID is required for a penalty")
                .When(e => string.Equals(e.EventType, "Penalty", StringComparison.OrdinalIgnoreCase));

            eventItem.RuleFor(e => e.PenaltyMinutes)
                .InclusiveBetween(2, 20).WithMessage("Penalty duration must be between 2 and 20 minutes")
                .When(e => string.Equals(e.EventType, "Penalty", StringComparison.OrdinalIgnoreCase));
        });
    }
}
