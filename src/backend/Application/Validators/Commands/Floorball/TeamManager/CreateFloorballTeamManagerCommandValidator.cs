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

        RuleFor(x => x.PrimaryResponsibility)
            .MaximumLength(100).WithMessage("Primary responsibility cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.PrimaryResponsibility));

        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience cannot be negative");
    }
} 