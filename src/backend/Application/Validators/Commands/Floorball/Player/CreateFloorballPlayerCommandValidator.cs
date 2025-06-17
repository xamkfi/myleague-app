using Application.Commands.Floorball.Player;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Player;

/// <summary>
/// Validator for CreateFloorballPlayerCommand
/// </summary>
public class CreateFloorballPlayerCommandValidator : AbstractValidator<CreateFloorballPlayerCommand>
{
    public CreateFloorballPlayerCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required")
            .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");
    }
} 