using Application.Features.Hockey.Officials.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Officials.Validators;

/// <summary>
/// Validator for <see cref="UpdateHockeyOfficialCommand"/>.
/// </summary>
public class UpdateHockeyOfficialCommandValidator : AbstractValidator<UpdateHockeyOfficialCommand>
{
    public UpdateHockeyOfficialCommandValidator()
    {
        RuleFor(x => x.OfficialId).NotEmpty().WithMessage("Official id is required.");
        RuleFor(x => x.OfficialRole).IsInEnum();
        RuleFor(x => x.OfficialNumber).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.OfficialNumber));
        RuleFor(x => x)
            .Must(x => x.LicenseIssueDate is null
                || x.LicenseExpiryDate is null
                || x.LicenseExpiryDate > x.LicenseIssueDate)
            .WithMessage("License expiry date must be after the issue date.");
    }
}
