using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class ClearHockeyMatchActiveGoalieCommandValidator : AbstractValidator<ClearHockeyMatchActiveGoalieCommand>
{
    public ClearHockeyMatchActiveGoalieCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
    }
}
