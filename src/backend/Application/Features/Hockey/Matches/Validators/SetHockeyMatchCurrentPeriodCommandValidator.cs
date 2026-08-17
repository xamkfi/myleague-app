using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class SetHockeyMatchCurrentPeriodCommandValidator : AbstractValidator<SetHockeyMatchCurrentPeriodCommand>
{
    public SetHockeyMatchCurrentPeriodCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(0);
    }
}
