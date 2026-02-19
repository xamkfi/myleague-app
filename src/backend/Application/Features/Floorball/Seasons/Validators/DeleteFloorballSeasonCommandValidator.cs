using Application.Features.Floorball.Seasons.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

/// <summary>
/// Validator for DeleteFloorballSeasonCommand
/// </summary>
public class DeleteFloorballSeasonCommandValidator : AbstractValidator<DeleteFloorballSeasonCommand>
{
    public DeleteFloorballSeasonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Season ID is required")
            .NotEqual(Guid.Empty).WithMessage("Season ID cannot be empty");
    }
} 