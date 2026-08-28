using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class SetHockeyMatchWentToOvertimeCommandValidator : AbstractValidator<SetHockeyMatchWentToOvertimeCommand>
{
    public SetHockeyMatchWentToOvertimeCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
    }
}
