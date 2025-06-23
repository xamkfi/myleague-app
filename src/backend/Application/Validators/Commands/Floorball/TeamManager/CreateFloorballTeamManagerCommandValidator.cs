using Application.Commands.Floorball.TeamManager;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.TeamManager;

/// <summary>
/// Validator for CreateFloorballTeamManagerCommand
/// </summary>
public class CreateFloorballTeamManagerCommandValidator : AbstractValidator<CreateFloorballTeamManagerCommand>
{
    public CreateFloorballTeamManagerCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required")
            .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");

        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
    }
} 