using Application.Commands.Floorball.Player;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Player;

/// <summary>
/// Validator for UpdateFloorballPlayerCommand
/// </summary>
public class UpdateFloorballPlayerCommandValidator : AbstractValidator<UpdateFloorballPlayerCommand>
{
    public UpdateFloorballPlayerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");
    }
} 