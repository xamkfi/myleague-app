using Application.Commands.Common;
using FluentValidation;

namespace Application.Validators.Commands.Common;

/// <summary>
/// Validator for ActivateDivisionCommand
/// </summary>
public class ActivateDivisionCommandValidator : AbstractValidator<ActivateDivisionCommand>
{
    public ActivateDivisionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Division ID is required");
    }
} 