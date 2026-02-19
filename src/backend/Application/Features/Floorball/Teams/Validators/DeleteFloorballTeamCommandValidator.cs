using Application.Features.Floorball.Teams.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Teams.Validators;

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
