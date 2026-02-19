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
            .InclusiveBetween(1, 5).WithMessage("Period number must be between 1 and 5 (4 for overtime, 5 for shootout)");

        RuleFor(x => x.TimeInSeconds)
            .InclusiveBetween(0, 1200).WithMessage("Time must be between 0 and 1200 seconds (20 minutes)");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
} 
