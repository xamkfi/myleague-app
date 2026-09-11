using Application.Features.Football.Referees.Commands;
using FluentValidation;

namespace Application.Features.Football.Referees.Validators;

/// <summary>
/// Validator for UpdateFootballRefereeCommand
/// </summary>
public class UpdateFootballRefereeCommandValidator : AbstractValidator<UpdateFootballRefereeCommand>
{
    public UpdateFootballRefereeCommandValidator()
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
        return date.HasValue && date.Value != default;
    }
}
