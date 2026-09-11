using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class SetHockeyMatchActiveGoalieCommandValidator : AbstractValidator<SetHockeyMatchActiveGoalieCommand>
{
    public SetHockeyMatchActiveGoalieCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchActivePlayerId).NotEmpty();
    }
}
