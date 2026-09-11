using Application.Features.Football.Players.Commands;
using FluentValidation;

namespace Application.Features.Football.Players.Validators;

/// <summary>
/// Validator for CreateFootballPlayerCommand
/// </summary>
public class CreateFootballPlayerCommandValidator : AbstractValidator<CreateFootballPlayerCommand>
{
    public CreateFootballPlayerCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required")
            .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");
    }
} 
