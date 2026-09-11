using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RecordHockeyGoalieChangeCommandValidator : AbstractValidator<RecordHockeyGoalieChangeCommand>
{
    public RecordHockeyGoalieChangeCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TimeInSeconds).GreaterThanOrEqualTo(0);
    }
}
