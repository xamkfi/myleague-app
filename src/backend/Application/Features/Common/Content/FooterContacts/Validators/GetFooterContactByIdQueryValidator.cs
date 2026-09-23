using Application.Features.Common.Content.FooterContacts.Queries;
using FluentValidation;

namespace Application.Features.Common.Content.FooterContacts.Validators;

public class GetFooterContactByIdQueryValidator : AbstractValidator<GetFooterContactByIdQuery>
{
    public GetFooterContactByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Contact ID is required");
    }
}
