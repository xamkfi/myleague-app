using Application.Commands.Floorball.TeamManager;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.TeamManager;

/// <summary>
/// Validator for UpdateFloorballTeamManagerCommand
/// </summary>
public class UpdateFloorballTeamManagerCommandValidator : AbstractValidator<UpdateFloorballTeamManagerCommand>
{
    public UpdateFloorballTeamManagerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Team manager ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team manager ID cannot be empty");

        RuleFor(x => x.PrimaryResponsibility)
            .MaximumLength(100).WithMessage("Primary responsibility cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.PrimaryResponsibility));
    }
} 