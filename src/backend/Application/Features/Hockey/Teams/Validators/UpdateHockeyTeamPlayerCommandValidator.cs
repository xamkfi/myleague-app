using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="UpdateHockeyTeamPlayerCommand"/>.
/// </summary>
public class UpdateHockeyTeamPlayerCommandValidator : AbstractValidator<UpdateHockeyTeamPlayerCommand>
{
    public UpdateHockeyTeamPlayerCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Position).IsInEnum();
        RuleFor(x => x.RosterStatus).IsInEnum();
        RuleFor(x => x.CaptainRole).IsInEnum();
    }
}
