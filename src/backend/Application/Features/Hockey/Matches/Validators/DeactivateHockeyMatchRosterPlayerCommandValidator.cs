using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class DeactivateHockeyMatchRosterPlayerCommandValidator : AbstractValidator<DeactivateHockeyMatchRosterPlayerCommand>
{
    public DeactivateHockeyMatchRosterPlayerCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchActivePlayerId).NotEmpty();
    }
}
