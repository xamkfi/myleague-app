using Application.Commands.Floorball.Coach;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Coach;

/// <summary>
/// Validator for DeleteFloorballCoachCommand
/// </summary>
public class DeleteFloorballCoachCommandValidator : AbstractValidator<DeleteFloorballCoachCommand>
{
    public DeleteFloorballCoachCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Coach ID is required")
            .NotEqual(Guid.Empty).WithMessage("Coach ID cannot be empty");
    }
} 