using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class AddHockeyMatchLinePlayerCommandValidator : AbstractValidator<AddHockeyMatchLinePlayerCommand>
{
    public AddHockeyMatchLinePlayerCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchLineId).NotEmpty();
        RuleFor(x => x.MatchActivePlayerId).NotEmpty();
        RuleFor(x => x.Slot!).IsInEnum().When(x => x.Slot.HasValue);
        RuleFor(x => x.Order!).GreaterThanOrEqualTo(0).When(x => x.Order.HasValue);
    }
}
