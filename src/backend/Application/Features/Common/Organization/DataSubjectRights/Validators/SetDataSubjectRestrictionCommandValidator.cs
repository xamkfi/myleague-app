using Application.Features.Common.Organization.DataSubjectRights.Commands;
using FluentValidation;

namespace Application.Features.Common.Organization.DataSubjectRights.Validators;

/// <summary>
/// Validator for <see cref="SetDataSubjectRestrictionCommand"/>.
/// </summary>
public class SetDataSubjectRestrictionCommandValidator : AbstractValidator<SetDataSubjectRestrictionCommand>
{
    public SetDataSubjectRestrictionCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required");
    }
}
