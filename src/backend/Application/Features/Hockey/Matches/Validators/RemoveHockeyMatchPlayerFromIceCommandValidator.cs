using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class RemoveHockeyMatchPlayerFromIceCommandValidator : AbstractValidator<RemoveHockeyMatchPlayerFromIceCommand>
{
    public RemoveHockeyMatchPlayerFromIceCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchActivePlayerId).NotEmpty();
    }
}
