using Application.Commands.Floorball.Team;
using Domain.Enums.Floorball;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Team;

/// <summary>
/// Validator for CreateFloorballTeamCommand
/// </summary>
public class CreateFloorballTeamCommandValidator : AbstractValidator<CreateFloorballTeamCommand>
{
    public CreateFloorballTeamCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(100).WithMessage("Team name cannot exceed 100 characters");

        RuleFor(x => x.Division)
            .NotNull().WithMessage("Division is required")
            .IsInEnum().WithMessage("Invalid division value");

        RuleFor(x => x.ClubId)
            .NotEmpty().WithMessage("Club ID is required")
            .NotEqual(Guid.Empty).WithMessage("Club ID cannot be empty");

        RuleFor(x => x.HomeArena)
            .NotEmpty().WithMessage("Home arena is required")
            .MaximumLength(100).WithMessage("Home arena name cannot exceed 100 characters");

        RuleFor(x => x.PrimaryJerseyColor)
            .NotEmpty().WithMessage("Primary jersey color is required")
            .MaximumLength(50).WithMessage("Primary jersey color cannot exceed 50 characters");

        RuleFor(x => x.TeamCategory)
            .NotNull().WithMessage("Team category is required")
            .IsInEnum().WithMessage("Invalid team category value");

        RuleFor(x => x.SecondaryJerseyColor)
            .MaximumLength(50).WithMessage("Secondary jersey color cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.SecondaryJerseyColor));
    }
} 
