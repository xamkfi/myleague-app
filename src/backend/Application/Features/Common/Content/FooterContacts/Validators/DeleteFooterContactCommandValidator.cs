using Application.Features.Common.Content.FooterContacts.Commands;
using FluentValidation;

namespace Application.Features.Common.Content.FooterContacts.Validators;

public class DeleteFooterContactCommandValidator : AbstractValidator<DeleteFooterContactCommand>
{
    public DeleteFooterContactCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Contact ID is required");
    }
}
