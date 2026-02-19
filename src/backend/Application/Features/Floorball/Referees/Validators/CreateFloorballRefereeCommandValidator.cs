using Application.Features.Floorball.Referees.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Referees.Validators;

/// <summary>
/// Validator for CreateFloorballRefereeCommand
/// </summary>
public class CreateFloorballRefereeCommandValidator : AbstractValidator<CreateFloorballRefereeCommand>
{
    public CreateFloorballRefereeCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required")
            .NotEqual(Guid.Empty).WithMessage("Person ID cannot be empty");

        RuleFor(x => x.LicenseIssueDate)
            .Must(BeValidDate).WithMessage("License issue date must be a valid date");

        RuleFor(x => x.LicenseExpiryDate)
            .Must(BeValidDate).WithMessage("License expiry date must be a valid date")
            .GreaterThan(x => x.LicenseIssueDate).WithMessage("License expiry date must be after issue date");
    }

    private bool BeValidDate(DateTime date)
    {
        return date != default && date <= DateTime.UtcNow.AddYears(10);
    }
} 
