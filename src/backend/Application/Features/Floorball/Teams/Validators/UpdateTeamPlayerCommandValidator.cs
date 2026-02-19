using Application.Commands.Floorball.Team;
using Domain.Enums.Floorball;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Team;

/// <summary>
/// Validator for UpdateTeamPlayerCommand
/// </summary>
public class UpdateTeamPlayerCommandValidator : AbstractValidator<UpdateTeamPlayerCommand>
{
    public UpdateTeamPlayerCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");

        RuleFor(x => x.PlayerId)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");

        RuleFor(x => x.Position)
            .NotNull().WithMessage("Position is required")
            .IsInEnum().WithMessage("Invalid position value");

        RuleFor(x => x.JerseyNumber)
            .InclusiveBetween(1, 99)
            .When(x => x.JerseyNumber.HasValue)
            .WithMessage("Jersey number must be between 1 and 99");
    }
} 