using Application.Commands.Common;
using FluentValidation;

namespace Application.Validators.Commands.Common;

/// <summary>
/// Validator for DeleteDivisionCommand
/// </summary>
public class DeleteDivisionCommandValidator : AbstractValidator<DeleteDivisionCommand>
{
    public DeleteDivisionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Division ID is required");
    }
} 