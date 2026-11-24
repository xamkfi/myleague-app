using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyMissedPenaltyShotCommandValidator : AbstractValidator<RecordHockeyMissedPenaltyShotCommand>
{
    public RecordHockeyMissedPenaltyShotCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.ShooterActivePlayerId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
    }
}

public class DeleteHockeyMissedPenaltyShotCommandValidator : AbstractValidator<DeleteHockeyMissedPenaltyShotCommand>
{
    public DeleteHockeyMissedPenaltyShotCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MissedPenaltyShotEventId).NotEmpty();
    }
}

public class SetHockeyMatchDecisionCommandValidator : AbstractValidator<SetHockeyMatchDecisionCommand>
{
    public SetHockeyMatchDecisionCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.Decision).IsInEnum();
    }
}
