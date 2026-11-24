using Application.Features.Football.Matches.Commands;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

/// <summary>
/// Validator for RecordFootballMissedPenaltyCommand
/// </summary>
public class RecordFootballMissedPenaltyCommandValidator : AbstractValidator<RecordFootballMissedPenaltyCommand>
{
    public RecordFootballMissedPenaltyCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team ID is required");
        RuleFor(x => x.ShooterPlayerId).NotEmpty().WithMessage("Shooter is required");
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1).WithMessage("Period number must be 1 or greater");
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0).WithMessage("Time must be non-negative");
    }
}

/// <summary>
/// Validator for DeleteFootballMissedPenaltyCommand
/// </summary>
public class DeleteFootballMissedPenaltyCommandValidator : AbstractValidator<DeleteFootballMissedPenaltyCommand>
{
    public DeleteFootballMissedPenaltyCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.MissedPenaltyEventId).NotEmpty().WithMessage("Event ID is required");
    }
}

/// <summary>
/// Validator for SetFootballMatchDecisionCommand
/// </summary>
public class SetFootballMatchDecisionCommandValidator : AbstractValidator<SetFootballMatchDecisionCommand>
{
    public SetFootballMatchDecisionCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.Decision).IsInEnum().WithMessage("Unknown match decision");
    }
}
