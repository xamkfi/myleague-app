using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyGoalCommandValidator : AbstractValidator<RecordHockeyGoalCommand>
{
    public RecordHockeyGoalCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.ScoringMatchTeamId).NotEmpty();
        RuleFor(x => x.ScorerActivePlayerId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GoalStrength).IsInEnum();
    }
}
