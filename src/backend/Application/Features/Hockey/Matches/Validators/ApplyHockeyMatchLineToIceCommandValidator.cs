using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class ApplyHockeyMatchLineToIceCommandValidator : AbstractValidator<ApplyHockeyMatchLineToIceCommand>
{
    public ApplyHockeyMatchLineToIceCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchLineId).NotEmpty();
    }
}
