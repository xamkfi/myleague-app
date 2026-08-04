using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class DisableHockeyMatchOnIceTrackingCommandValidator : AbstractValidator<DisableHockeyMatchOnIceTrackingCommand>
{
    public DisableHockeyMatchOnIceTrackingCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
    }
}
