using Application.Features.Common.Divisions.Commands;
using Application.Features.Common.MatchTimer.Commands;
using Application.Features.Common.Images.Commands;
using FluentValidation;

namespace Application.Features.Common.Divisions.Validators;

/// <summary>
/// Validator for UpdateDivisionCommand
/// </summary>
public class UpdateDivisionCommandValidator : AbstractValidator<UpdateDivisionCommand>
{
    public UpdateDivisionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Division ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Division name is required")
            .MaximumLength(100).WithMessage("Division name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Division description is required")
            .MaximumLength(500).WithMessage("Division description cannot exceed 500 characters");

        RuleFor(x => x.Level)
            .GreaterThan(0).WithMessage("Division level must be greater than 0")
            .LessThanOrEqualTo(10).WithMessage("Division level cannot exceed 10");
    }
} 
