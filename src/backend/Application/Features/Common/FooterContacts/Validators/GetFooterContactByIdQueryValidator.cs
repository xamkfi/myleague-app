using Application.Features.Common.FooterContacts.Queries;
using FluentValidation;

namespace Application.Features.Common.FooterContacts.Validators;

public class GetFooterContactByIdQueryValidator : AbstractValidator<GetFooterContactByIdQuery>
{
    public GetFooterContactByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Contact ID is required");
    }
}
