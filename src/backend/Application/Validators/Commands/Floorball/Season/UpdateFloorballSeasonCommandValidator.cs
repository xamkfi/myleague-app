using Application.Commands.Floorball.Season;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Season;

/// <summary>
/// Validator for UpdateFloorballSeasonCommand
/// </summary>
public class UpdateFloorballSeasonCommandValidator : AbstractValidator<UpdateFloorballSeasonCommand>
{
    public UpdateFloorballSeasonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Season ID is required")
            .NotEqual(Guid.Empty).WithMessage("Season ID cannot be empty");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Season name is required")
            .MaximumLength(100).WithMessage("Season name cannot exceed 100 characters");

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
        return date != default;
    }
} 