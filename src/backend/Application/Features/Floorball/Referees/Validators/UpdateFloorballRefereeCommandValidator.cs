using Application.Features.Floorball.Referees.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Referees.Validators;

/// <summary>
/// Validator for UpdateFloorballRefereeCommand
/// </summary>
public class UpdateFloorballRefereeCommandValidator : AbstractValidator<UpdateFloorballRefereeCommand>
{
    public UpdateFloorballRefereeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Referee ID is required")
            .NotEqual(Guid.Empty).WithMessage("Referee ID cannot be empty");

        RuleFor(x => x.LicenseIssueDate)
            .Must(BeValidDate).WithMessage("License issue date must be a valid date")
            .When(x => x.LicenseIssueDate.HasValue);

        RuleFor(x => x.LicenseExpiryDate)
            .Must(BeValidDate).WithMessage("License expiry date must be a valid date")
            .GreaterThan(x => x.LicenseIssueDate).WithMessage("License expiry date must be after issue date")
            .When(x => x.LicenseExpiryDate.HasValue && x.LicenseIssueDate.HasValue);

        RuleFor(x => x.MatchesOfficiated)
            .GreaterThanOrEqualTo(0).WithMessage("Matches officiated cannot be negative");
    }

    private bool BeValidDate(DateTime? date)
    {
        return date.HasValue && date.Value.Kind == DateTimeKind.Utc;
    }
} 
