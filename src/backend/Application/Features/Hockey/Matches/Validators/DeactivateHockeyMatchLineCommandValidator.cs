using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class DeactivateHockeyMatchLineCommandValidator : AbstractValidator<DeactivateHockeyMatchLineCommand>
{
    public DeactivateHockeyMatchLineCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchLineId).NotEmpty();
    }
}
