using Application.Features.Floorball.Players.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Players.Validators;

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
