using Application.Commands.Common;
using FluentValidation;

namespace Application.Validators.Commands.Common;

/// <summary>
/// Validator for DeactivateDivisionCommand
/// </summary>
public class DeactivateDivisionCommandValidator : AbstractValidator<DeactivateDivisionCommand>
{
    public DeactivateDivisionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Division ID is required");
    }
} 