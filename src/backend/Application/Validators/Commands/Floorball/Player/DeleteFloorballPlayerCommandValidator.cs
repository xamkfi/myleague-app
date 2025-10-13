using Application.Commands.Floorball.Player;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Player;

/// <summary>
/// Validator for DeleteFloorballPlayerCommand
/// </summary>
public class DeleteFloorballPlayerCommandValidator : AbstractValidator<DeleteFloorballPlayerCommand>
{
    public DeleteFloorballPlayerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");
    }
} 