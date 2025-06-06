using Application.Commands.NewsArticles;
using FluentValidation;

namespace Application.Validators.Commands.NewsArticles;

/// <summary>
/// Validator for SetNewsArticleImageCommand
/// </summary>
public class SetNewsArticleImageCommandValidator : AbstractValidator<SetNewsArticleImageCommand>
{
    public SetNewsArticleImageCommandValidator()
    {
        RuleFor(x => x.NewsId)
            .NotEmpty().WithMessage("News article ID is required")
            .NotEqual(Guid.Empty).WithMessage("News article ID cannot be empty");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required")
            .Must(BeValidUrl).WithMessage("Invalid image URL format");
    }

    private static bool BeValidUrl(string? url)
    {
        return !string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
} 