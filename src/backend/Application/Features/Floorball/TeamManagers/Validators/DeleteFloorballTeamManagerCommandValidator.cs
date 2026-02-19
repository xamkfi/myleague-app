using Application.Commands.Floorball.TeamManager;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.TeamManager;

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