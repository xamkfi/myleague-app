using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="RemovePlayerFromHockeyLineCommand"/>.
/// </summary>
public class RemovePlayerFromHockeyLineCommandValidator : AbstractValidator<RemovePlayerFromHockeyLineCommand>
{
    public RemovePlayerFromHockeyLineCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.TeamPlayerId).NotEmpty();
    }
}
