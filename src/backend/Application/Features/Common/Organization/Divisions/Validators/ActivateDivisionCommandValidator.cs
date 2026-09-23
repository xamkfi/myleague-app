using Application.Features.Common.Organization.Divisions.Commands;
using Application.Features.Common.CrossCutting.MatchTimer.Commands;
using Application.Features.Common.Content.Images.Commands;
using FluentValidation;

namespace Application.Features.Common.Organization.Divisions.Validators;

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
