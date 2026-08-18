using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RemoveHockeyMatchLinePlayerCommandValidator : AbstractValidator<RemoveHockeyMatchLinePlayerCommand>
{
    public RemoveHockeyMatchLinePlayerCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchLineId).NotEmpty();
        RuleFor(x => x.MatchActivePlayerId).NotEmpty();
    }
}
