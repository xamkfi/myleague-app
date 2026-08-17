using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class AddHockeyPeriodScoreCommandValidator : AbstractValidator<AddHockeyPeriodScoreCommand>
{
    public AddHockeyPeriodScoreCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.PeriodNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PeriodType).IsInEnum();
    }
}
