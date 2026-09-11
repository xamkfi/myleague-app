using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class AddHockeyMatchOfficialCommandValidator : AbstractValidator<AddHockeyMatchOfficialCommand>
{
    public AddHockeyMatchOfficialCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.OfficialId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
    }
}
