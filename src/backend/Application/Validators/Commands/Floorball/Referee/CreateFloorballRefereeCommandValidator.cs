using Application.Commands.Floorball.Referee;
using FluentValidation;

namespace Application.Validators.Commands.Floorball.Referee;

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
            .Must(BeValidDate).WithMessage("License issue date must be a valid date and in UTC");

        RuleFor(x => x.LicenseExpiryDate)
            .Must(BeValidDate).WithMessage("License expiry date must be a valid date and in UTC")
            .GreaterThan(x => x.LicenseIssueDate).WithMessage("License expiry date must be after issue date");
    }

    private bool BeValidDate(DateTime date)
    {
        return date.Kind == DateTimeKind.Utc && date <= DateTime.UtcNow.AddYears(10);
    }
} 