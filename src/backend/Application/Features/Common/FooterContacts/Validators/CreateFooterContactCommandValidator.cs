using Application.Features.Common.FooterContacts.Commands;
using FluentValidation;

namespace Application.Features.Common.FooterContacts.Validators;

public class CreateFooterContactCommandValidator : AbstractValidator<CreateFooterContactCommand>
{
    public CreateFooterContactCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Contact title is required")
            .MaximumLength(200).WithMessage("Contact title cannot exceed 200 characters");

        RuleFor(x => x.Details)
            .MaximumLength(500).WithMessage("Details cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Details));

        RuleFor(x => x.Email)
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters")
            .Must(ContainAt).WithMessage("Email must contain '@'")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Url)
            .MaximumLength(500).WithMessage("Url cannot exceed 500 characters")
            .Must(BeHttpUrl).WithMessage("Url must be an http or https address")
            .When(x => !string.IsNullOrWhiteSpace(x.Url));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative");

        RuleFor(x => x.Section)
            .IsInEnum().WithMessage("Footer section is not valid");
    }

    private static bool ContainAt(string? email)
    {
        return email is not null && email.Contains('@', StringComparison.Ordinal);
    }

    private static bool BeHttpUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrWhiteSpace(uri.Host);
    }
}
