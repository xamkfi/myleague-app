using Application.Features.Common.Organization.DataSubjectRights.Commands;
using FluentValidation;

namespace Application.Features.Common.Organization.DataSubjectRights.Validators;

/// <summary>
/// Validator for <see cref="EraseDataSubjectCommand"/>.
/// </summary>
public class EraseDataSubjectCommandValidator : AbstractValidator<EraseDataSubjectCommand>
{
    public EraseDataSubjectCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required");
    }
}
