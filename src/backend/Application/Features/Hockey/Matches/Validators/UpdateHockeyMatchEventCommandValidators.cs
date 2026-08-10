using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class UpdateHockeyGoalCommandValidator : AbstractValidator<UpdateHockeyGoalCommand>
{
    public UpdateHockeyGoalCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.GoalEventId).NotEmpty();
        RuleFor(x => x.ScoringMatchTeamId).NotEmpty();
        RuleFor(x => x.ScorerActivePlayerId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GoalStrength).IsInEnum();
    }
}

public class UpdateHockeyPenaltyCommandValidator : AbstractValidator<UpdateHockeyPenaltyCommand>
{
    public UpdateHockeyPenaltyCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.PenaltyEventId).NotEmpty();
        RuleFor(x => x.PenaltyMatchTeamId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Offence).IsInEnum();
        RuleFor(x => x.PenaltyMinutes).GreaterThanOrEqualTo(0);
    }
}

public class UpdateHockeyShotCommandValidator : AbstractValidator<UpdateHockeyShotCommand>
{
    public UpdateHockeyShotCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.ShotEventId).NotEmpty();
        RuleFor(x => x.ShootingMatchTeamId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShotResult).IsInEnum();
    }
}
