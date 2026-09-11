using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class LockHockeyMatchLineCommandValidator : AbstractValidator<LockHockeyMatchLineCommand>
{
    public LockHockeyMatchLineCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchLineId).NotEmpty();
    }
}
