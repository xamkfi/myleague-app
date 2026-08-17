using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="RemoveHockeyTeamStaffCommand"/>.
/// </summary>
public class RemoveHockeyTeamStaffCommandValidator : AbstractValidator<RemoveHockeyTeamStaffCommand>
{
    public RemoveHockeyTeamStaffCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.StaffId).NotEmpty();
    }
}
