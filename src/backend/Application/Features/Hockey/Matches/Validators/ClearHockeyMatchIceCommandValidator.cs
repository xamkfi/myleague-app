using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class ClearHockeyMatchIceCommandValidator : AbstractValidator<ClearHockeyMatchIceCommand>
{
    public ClearHockeyMatchIceCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
    }
}
