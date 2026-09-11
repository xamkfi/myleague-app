using Application.Features.Floorball.Players.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Players.Validators;

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
