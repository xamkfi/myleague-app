using Application.Commands.Clubs;
using FluentValidation;

namespace Application.Validators.Commands.Club;

/// <summary>
/// Validator for UpdateClubCommand
/// </summary>
public class UpdateClubCommandValidator : AbstractValidator<UpdateClubCommand>
{
    public UpdateClubCommandValidator()
    {
        RuleFor(x => x.ClubId)
            .NotEmpty().WithMessage("Club ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Club name is required")
            .MaximumLength(100).WithMessage("Club name cannot exceed 100 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(50).WithMessage("City cannot exceed 50 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(50).WithMessage("Country cannot exceed 50 characters");

        RuleFor(x => x.FoundingDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Founding date cannot be in the future");

        RuleFor(x => x.WebsiteUrl)
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
            .WithMessage("Invalid website URL format");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("Invalid email format");

        RuleFor(x => x.LogoUrl)
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("Invalid logo URL format");
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
} 