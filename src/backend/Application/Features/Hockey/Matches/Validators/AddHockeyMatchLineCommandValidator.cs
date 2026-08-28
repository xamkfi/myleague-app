using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class AddHockeyMatchLineCommandValidator : AbstractValidator<AddHockeyMatchLineCommand>
{
    public AddHockeyMatchLineCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchTeamId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LineType).IsInEnum();
        RuleFor(x => x.LineNumber!).GreaterThanOrEqualTo(0).When(x => x.LineNumber.HasValue);
    }
}
