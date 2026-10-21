using Application.Features.Common.Content.Advertisements.Commands;
using FluentValidation;

namespace Application.Features.Common.Content.Advertisements.Validators;

public class DeleteAdvertisementCommandValidator : AbstractValidator<DeleteAdvertisementCommand>
{
    public DeleteAdvertisementCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Advertisement ID is required");
    }
}
