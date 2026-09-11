using Application.Features.Floorball.TeamManagers.Commands;
using FluentValidation;

namespace Application.Features.Floorball.TeamManagers.Validators;

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
