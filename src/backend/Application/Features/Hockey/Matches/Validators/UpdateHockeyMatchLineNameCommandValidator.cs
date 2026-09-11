using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class UpdateHockeyMatchLineNameCommandValidator : AbstractValidator<UpdateHockeyMatchLineNameCommand>
{
    public UpdateHockeyMatchLineNameCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchLineId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
