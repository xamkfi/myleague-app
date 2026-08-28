using Application.Features.Football.Matches.Commands;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

/// <summary>
/// Validator for <see cref="ImportFootballMatchEventsCommand"/>.
/// </summary>
public class ImportFootballMatchEventsCommandValidator : AbstractValidator<ImportFootballMatchEventsCommand>
{
    public const int MaxEvents = 200;

    public ImportFootballMatchEventsCommandValidator()
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
                    || string.Equals(type, "Card", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Event type must be Goal or Card");

            eventItem.RuleFor(e => e.TeamId)
                .NotEmpty().WithMessage("Team ID is required")
                .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");

            eventItem.RuleFor(e => e.PeriodNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Period number must be 1 or greater");

            eventItem.RuleFor(e => e.TimeInSeconds)
                .GreaterThanOrEqualTo(0).WithMessage("Time must be non-negative");

            eventItem.RuleFor(e => e.PlayerId)
                .NotEmpty().WithMessage("Player ID is required")
                .When(e =>
                    string.Equals(e.EventType, "Goal", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(e.EventType, "Card", StringComparison.OrdinalIgnoreCase));

            eventItem.RuleFor(e => e.CardType)
                .NotNull().WithMessage("Card type is required for a card event")
                .When(e => string.Equals(e.EventType, "Card", StringComparison.OrdinalIgnoreCase));
        });
    }
}
