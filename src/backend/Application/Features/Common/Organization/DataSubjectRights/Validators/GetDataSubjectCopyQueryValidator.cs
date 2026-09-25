using Application.Features.Common.Organization.DataSubjectRights.Queries;
using FluentValidation;

namespace Application.Features.Common.Organization.DataSubjectRights.Validators;

/// <summary>
/// Validator for <see cref="GetDataSubjectCopyQuery"/>.
/// </summary>
public class GetDataSubjectCopyQueryValidator : AbstractValidator<GetDataSubjectCopyQuery>
{
    public GetDataSubjectCopyQueryValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required");
    }
}
