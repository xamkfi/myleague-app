using Application.Commands.Floorball.Coach;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Coach;

/// <summary>
/// Validator for UpdateFloorballCoachCommand
/// </summary>
public class UpdateFloorballCoachCommandValidator : AbstractValidator<UpdateFloorballCoachCommand>
{
    public UpdateFloorballCoachCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Coach ID is required")
            .NotEqual(Guid.Empty).WithMessage("Coach ID cannot be empty");

        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience cannot be negative");

        RuleFor(x => x.CertificationLevel)
            .MaximumLength(50).WithMessage("Certification level cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.CertificationLevel));

        RuleFor(x => x.Specialization)
            .MaximumLength(100).WithMessage("Specialization cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Specialization));
    }
} 