using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="AddPlayerToHockeyTeamCommand"/>.
/// </summary>
public class AddPlayerToHockeyTeamCommandValidator : AbstractValidator<AddPlayerToHockeyTeamCommand>
{
    public AddPlayerToHockeyTeamCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Position).IsInEnum();
        RuleFor(x => x.RosterStatus).IsInEnum();
    }
}
