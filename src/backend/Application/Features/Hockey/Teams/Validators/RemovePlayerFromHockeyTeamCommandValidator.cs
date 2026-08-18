using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="RemovePlayerFromHockeyTeamCommand"/>.
/// </summary>
public class RemovePlayerFromHockeyTeamCommandValidator : AbstractValidator<RemovePlayerFromHockeyTeamCommand>
{
    public RemovePlayerFromHockeyTeamCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
