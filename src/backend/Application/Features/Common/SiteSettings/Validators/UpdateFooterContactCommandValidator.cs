using Application.Features.Common.SiteSettings.Commands;
using FluentValidation;

namespace Application.Features.Common.SiteSettings.Validators;

/// <summary>
/// Validator for <see cref="UpdateFooterContactCommand"/>.
/// </summary>
public class UpdateFooterContactCommandValidator : AbstractValidator<UpdateFooterContactCommand>
{
    /// <summary>
    /// Initializes validation rules.
    /// </summary>
    public UpdateFooterContactCommandValidator()
    {
        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200);

        RuleFor(x => x.OrganizationAddress)
            .NotEmpty().WithMessage("Organization address is required.")
            .MaximumLength(500);

        RuleFor(x => x.ContactPersons)
            .NotNull()
            .Must(x => x.Count > 0).WithMessage("At least one contact person is required.");

        RuleForEach(x => x.ContactPersons)
            .ChildRules(person =>
            {
                person.RuleFor(p => p.NameOrRole).NotEmpty().MaximumLength(200);
                person.RuleFor(p => p.Email).NotEmpty().EmailAddress().MaximumLength(200);
                person.RuleFor(p => p.Phone).NotEmpty().MaximumLength(100);
            });
    }
}
