using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class MarkHockeyMatchStartedCommandValidator : AbstractValidator<MarkHockeyMatchStartedCommand>
{
    public MarkHockeyMatchStartedCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
    }
}
