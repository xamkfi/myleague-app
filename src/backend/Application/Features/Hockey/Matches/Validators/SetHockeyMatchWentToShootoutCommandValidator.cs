using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class SetHockeyMatchWentToShootoutCommandValidator : AbstractValidator<SetHockeyMatchWentToShootoutCommand>
{
    public SetHockeyMatchWentToShootoutCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
    }
}
