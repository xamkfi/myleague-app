using Application.Features.Floorball.Players.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Players.Validators;

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
