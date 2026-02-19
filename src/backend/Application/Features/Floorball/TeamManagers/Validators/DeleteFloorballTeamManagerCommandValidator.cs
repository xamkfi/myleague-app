using Application.Features.Floorball.TeamManagers.Commands;
using FluentValidation;

namespace Application.Features.Floorball.TeamManagers.Validators;

/// <summary>
/// Validator for DeleteFloorballTeamManagerCommand
/// </summary>
public class DeleteFloorballTeamManagerCommandValidator : AbstractValidator<DeleteFloorballTeamManagerCommand>
{
    public DeleteFloorballTeamManagerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Team manager ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team manager ID cannot be empty");
    }
} 
