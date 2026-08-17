using Application.Features.Football.Players.Commands;
using FluentValidation;

namespace Application.Features.Football.Players.Validators;

/// <summary>
/// Validator for DeleteFootballPlayerCommand
/// </summary>
public class DeleteFootballPlayerCommandValidator : AbstractValidator<DeleteFootballPlayerCommand>
{
    public DeleteFootballPlayerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");
    }
} 
