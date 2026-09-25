using Application.Features.Common.Organization.DataSubjectRights.Commands;
using FluentValidation;

namespace Application.Features.Common.Organization.DataSubjectRights.Validators;

/// <summary>
/// Validator for <see cref="RectifyDataSubjectCommand"/>.
/// </summary>
public class RectifyDataSubjectCommandValidator : AbstractValidator<RectifyDataSubjectCommand>
{
    public RectifyDataSubjectCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.BirthDate)
            .Must(birthDate => !birthDate.HasValue || birthDate.Value <= DateTime.UtcNow)
            .WithMessage("Birth date cannot be in the future.");

        RuleFor(x => x.AccountEmail)
            .MaximumLength(255)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.AccountEmail));
    }
}
