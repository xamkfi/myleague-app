using Application.Commands.Floorball.Season;
using Domain.Enums.Floorball;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Season;

/// <summary>
/// Validator for CreateFloorballSeasonCommand
/// </summary>
public class CreateFloorballSeasonCommandValidator : AbstractValidator<CreateFloorballSeasonCommand>
{
    public CreateFloorballSeasonCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Season name is required")
            .MaximumLength(100).WithMessage("Season name cannot exceed 100 characters");

        RuleFor(x => x.Division)
            .NotNull().WithMessage("Division is required")
            .IsInEnum().WithMessage("Invalid division value");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .Must(BeValidDate).WithMessage("Start date must be a valid date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .Must(BeValidDate).WithMessage("End date must be a valid date")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }

    private bool BeValidDate(DateTime date)
    {
        return date != default && date.Kind == DateTimeKind.Utc;
    }
} 