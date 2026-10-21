using Application.Features.Common.Content.Advertisements.Commands;
using Domain.Entities.Common;
using FluentValidation;

namespace Application.Features.Common.Content.Advertisements.Validators;

public class CreateAdvertisementCommandValidator : AbstractValidator<CreateAdvertisementCommand>
{
    public CreateAdvertisementCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Advertisement name is required")
            .MaximumLength(Advertisement.NameMaxLength)
            .WithMessage($"Advertisement name cannot exceed {Advertisement.NameMaxLength} characters");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Advertisement image is required")
            .MaximumLength(Advertisement.UrlMaxLength)
            .WithMessage($"Image url cannot exceed {Advertisement.UrlMaxLength} characters")
            .Must(BeHttpUrl).WithMessage("Image url must be an http or https address");

        RuleFor(x => x.LinkUrl)
            .MaximumLength(Advertisement.UrlMaxLength)
            .WithMessage($"Link url cannot exceed {Advertisement.UrlMaxLength} characters")
            .Must(BeHttpUrl).WithMessage("Link url must be an http or https address")
            .When(x => !string.IsNullOrWhiteSpace(x.LinkUrl));

        RuleFor(x => x.AltText)
            .MaximumLength(Advertisement.AltTextMaxLength)
            .WithMessage($"Alt text cannot exceed {Advertisement.AltTextMaxLength} characters");

        RuleFor(x => x.Slot)
            .IsInEnum().WithMessage("Advertisement slot is not valid");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative");

        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt).WithMessage("End time must be after the start time")
            .When(x => x.StartsAt.HasValue && x.EndsAt.HasValue);
    }

    private static bool BeHttpUrl(string? url)
    {
        return !string.IsNullOrWhiteSpace(url)
            && Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrWhiteSpace(uri.Host);
    }
}
