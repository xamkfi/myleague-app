using Application.Commands.Floorball.Season;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Season;

/// <summary>
/// Validator for CompleteFloorballSeasonCommand
/// </summary>
public class CompleteFloorballSeasonCommandValidator : AbstractValidator<CompleteFloorballSeasonCommand>
{
    public CompleteFloorballSeasonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Season ID is required")
            .NotEqual(Guid.Empty).WithMessage("Season ID cannot be empty");
    }
} 