using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validator for RecordGoalCommand
/// </summary>
public class RecordGoalCommandValidator : AbstractValidator<RecordGoalCommand>
{
    public RecordGoalCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");

        RuleFor(x => x.ScoringTeamId)
            .NotEmpty().WithMessage("Scoring team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Scoring team ID cannot be empty");

        RuleFor(x => x.ScoringPlayerId)
            .NotEmpty().WithMessage("Scoring player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Scoring player ID cannot be empty");

        RuleFor(x => x.AssistingPlayerId)
            .NotEqual(Guid.Empty).WithMessage("Assisting player ID cannot be empty")
            .When(x => x.AssistingPlayerId.HasValue);

        RuleFor(x => x.PeriodNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Period number must be 1 or greater");

        // We intentionally only enforce a non-negative floor on the timestamp. With the
        // continuous match clock (period 2 begins at 15:00, period 3 at 30:00, etc.),
        // any upper bound tied to a single period length would be wrong, and arbitrary
        // global caps caused real workflows to fail. The scorekeeper is trusted to
        // enter sensible values for goals; the UI prefills the live clock for them.
        RuleFor(x => x.TimeInSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Time must be non-negative");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
} 
