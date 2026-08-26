using Application.Features.Hockey.Players.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Players.Validators;

/// <summary>
/// Validator for <see cref="DeleteHockeyPlayerCommand"/>.
/// </summary>
public class DeleteHockeyPlayerCommandValidator : AbstractValidator<DeleteHockeyPlayerCommand>
{
    public DeleteHockeyPlayerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");
    }
}
