using Application.Commands.Floorball.Team;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Team;

/// <summary>
/// Validator for RemovePlayerFromTeamCommand
/// </summary>
public class RemovePlayerFromTeamCommandValidator : AbstractValidator<RemovePlayerFromTeamCommand>
{
    public RemovePlayerFromTeamCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");

        RuleFor(x => x.PlayerId)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");
    }
} 