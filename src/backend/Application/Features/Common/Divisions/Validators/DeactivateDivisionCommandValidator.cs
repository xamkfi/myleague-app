using Application.Features.Common.Divisions.Commands;
using Application.Features.Common.MatchTimer.Commands;
using Application.Features.Common.Images.Commands;
using FluentValidation;

namespace Application.Features.Common.Divisions.Validators;

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
