using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="AddHockeyTeamStaffCommand"/>.
/// </summary>
public class AddHockeyTeamStaffCommandValidator : AbstractValidator<AddHockeyTeamStaffCommand>
{
    public AddHockeyTeamStaffCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
    }
}
