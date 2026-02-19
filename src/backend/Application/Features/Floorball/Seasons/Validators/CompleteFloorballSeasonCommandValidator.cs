using Application.Features.Floorball.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

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