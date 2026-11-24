using Application.Features.Floorball.Matches.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validator for RecordFloorballMissedPenaltyCommand
/// </summary>
public class RecordFloorballMissedPenaltyCommandValidator : AbstractValidator<RecordFloorballMissedPenaltyCommand>
{
    public RecordFloorballMissedPenaltyCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team ID is required");
        RuleFor(x => x.ShooterPlayerId).NotEmpty().WithMessage("Shooter is required");
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1).WithMessage("Period number must be 1 or greater");
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0).WithMessage("Time must be non-negative");
    }
}

/// <summary>
/// Validator for DeleteFloorballMissedPenaltyCommand
/// </summary>
public class DeleteFloorballMissedPenaltyCommandValidator : AbstractValidator<DeleteFloorballMissedPenaltyCommand>
{
    public DeleteFloorballMissedPenaltyCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.MissedPenaltyEventId).NotEmpty().WithMessage("Event ID is required");
    }
}

/// <summary>
/// Validator for SetFloorballMatchDecisionCommand
/// </summary>
public class SetFloorballMatchDecisionCommandValidator : AbstractValidator<SetFloorballMatchDecisionCommand>
{
    public SetFloorballMatchDecisionCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty().WithMessage("Match ID is required");
        RuleFor(x => x.Decision).IsInEnum().WithMessage("Unknown match decision");
    }
}
