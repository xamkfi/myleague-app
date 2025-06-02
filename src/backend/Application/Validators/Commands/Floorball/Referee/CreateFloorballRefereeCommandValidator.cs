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

        RuleFor(x => x.LicenseIssuedDate)
            .Must(BeValidDate).WithMessage("License issued date must be a valid date")
            .When(x => x.LicenseIssuedDate.HasValue);

        RuleFor(x => x.LicenseExpiryDate)
            .Must(BeValidDate).WithMessage("License expiry date must be a valid date")
            .GreaterThan(x => x.LicenseIssuedDate).WithMessage("License expiry date must be after issue date")
            .When(x => x.LicenseExpiryDate.HasValue && x.LicenseIssuedDate.HasValue);

        RuleFor(x => x.MatchesOfficiated)
            .GreaterThanOrEqualTo(0).WithMessage("Matches officiated cannot be negative");
    }

    private bool BeValidDate(DateTime? date)
    {
        return date.HasValue && date.Value.Kind == DateTimeKind.Utc;
    }
} 