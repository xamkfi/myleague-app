using Application.Commands.Floorball.Team;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Team;

/// <summary>
/// Validator for DeleteFloorballTeamCommand
/// </summary>
public class DeleteFloorballTeamCommandValidator : AbstractValidator<DeleteFloorballTeamCommand>
{
    public DeleteFloorballTeamCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
    }
} 