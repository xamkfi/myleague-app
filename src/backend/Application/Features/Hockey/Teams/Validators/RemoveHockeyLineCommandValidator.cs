using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="RemoveHockeyLineCommand"/>.
/// </summary>
public class RemoveHockeyLineCommandValidator : AbstractValidator<RemoveHockeyLineCommand>
{
    public RemoveHockeyLineCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
    }
}
