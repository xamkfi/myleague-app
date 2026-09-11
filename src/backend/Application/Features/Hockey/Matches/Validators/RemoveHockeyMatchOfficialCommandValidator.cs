using Application.Features.Hockey.Matches.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

/// <summary>
/// Validator for <see cref="RemoveHockeyMatchOfficialCommand"/>.
/// </summary>
public class RemoveHockeyMatchOfficialCommandValidator : AbstractValidator<RemoveHockeyMatchOfficialCommand>
{
    public RemoveHockeyMatchOfficialCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.OfficialId).NotEmpty();
    }
}
