using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class EnableHockeyMatchOnIceTrackingCommandValidator : AbstractValidator<EnableHockeyMatchOnIceTrackingCommand>
{
    public EnableHockeyMatchOnIceTrackingCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
    }
}
