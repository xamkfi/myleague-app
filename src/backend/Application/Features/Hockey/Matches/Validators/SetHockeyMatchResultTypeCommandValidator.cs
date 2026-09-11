using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class SetHockeyMatchResultTypeCommandValidator : AbstractValidator<SetHockeyMatchResultTypeCommand>
{
    public SetHockeyMatchResultTypeCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.ResultType!).IsInEnum().When(x => x.ResultType.HasValue);
    }
}
