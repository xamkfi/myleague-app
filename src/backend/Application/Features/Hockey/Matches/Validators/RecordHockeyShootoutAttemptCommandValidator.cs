using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyShootoutAttemptCommandValidator : AbstractValidator<RecordHockeyShootoutAttemptCommand>
{
    public RecordHockeyShootoutAttemptCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.ShooterActivePlayerId).NotEmpty();
        RuleFor(x => x.GoalieActivePlayerId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShotOrder).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Result).IsInEnum();
    }
}
