using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class SetHockeyMatchTeamGoalsCommandValidator : AbstractValidator<SetHockeyMatchTeamGoalsCommand>
{
    public SetHockeyMatchTeamGoalsCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.TeamSlot).IsInEnum();
        RuleFor(x => x.Goals).GreaterThanOrEqualTo(0);
    }
}
