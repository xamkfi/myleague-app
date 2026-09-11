using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class AddHockeyMatchPlayerToIceCommandValidator : AbstractValidator<AddHockeyMatchPlayerToIceCommand>
{
    public AddHockeyMatchPlayerToIceCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.MatchActivePlayerId).NotEmpty();
        RuleFor(x => x.Slot!).IsInEnum().When(x => x.Slot.HasValue);
        RuleFor(x => x.PeriodNumber!).GreaterThanOrEqualTo(1).When(x => x.PeriodNumber.HasValue);
        RuleFor(x => x.TimeInSeconds!).GreaterThanOrEqualTo(0).When(x => x.TimeInSeconds.HasValue);
    }
}
