using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="SetHockeyTeamActiveStatusCommand"/>.
/// </summary>
public class SetHockeyTeamActiveStatusCommandValidator : AbstractValidator<SetHockeyTeamActiveStatusCommand>
{
    public SetHockeyTeamActiveStatusCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team id is required.");
    }
}
