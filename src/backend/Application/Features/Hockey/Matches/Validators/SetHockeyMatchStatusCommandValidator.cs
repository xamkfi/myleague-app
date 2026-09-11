using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class SetHockeyMatchStatusCommandValidator : AbstractValidator<SetHockeyMatchStatusCommand>
{
    public SetHockeyMatchStatusCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
