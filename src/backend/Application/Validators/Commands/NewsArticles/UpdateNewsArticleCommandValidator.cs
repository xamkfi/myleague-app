using Application.Commands.NewsArticles;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Validators.Commands.NewsArticles;

/// <summary>
/// Validator for UpdateNewsArticleCommand
/// </summary>
public class UpdateNewsArticleCommandValidator : AbstractValidator<UpdateNewsArticleCommand>
{
    public UpdateNewsArticleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("News article ID is required")
            .NotEqual(Guid.Empty).WithMessage("News article ID cannot be empty");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.ContentHtml)
            .NotEmpty().WithMessage("Content is required");

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Summary cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Summary));

        RuleFor(x => x.Author)
            .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Author));

        RuleFor(x => x.Category)
            .Must(BeValidNewsCategory).WithMessage("Invalid news category")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.SportCategory)
            .Must(BeValidSportCategory).WithMessage("Invalid sport category")
            .When(x => !string.IsNullOrEmpty(x.SportCategory));

        RuleForEach(x => x.ImageUrls)
            .Must(BeValidUrl).WithMessage("Invalid image URL format")
            .When(x => x.ImageUrls != null && x.ImageUrls.Any());

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag cannot be empty")
            .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters")
            .When(x => x.Tags != null && x.Tags.Any());

        RuleFor(x => x.Tags)
            .Must(HaveUniqueItems).WithMessage("Duplicate tags are not allowed")
            .When(x => x.Tags != null && x.Tags.Any());
    }

    private static bool BeValidNewsCategory(string? category)
    {
        return !string.IsNullOrEmpty(category) && Enum.TryParse<NewsCategory>(category, true, out _);
    }

    private static bool BeValidSportCategory(string? category)
    {
        return !string.IsNullOrEmpty(category) && Enum.TryParse<SportsCategory>(category, true, out _);
    }

    private static bool BeValidUrl(string? url)
    {
        return !string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private static bool HaveUniqueItems(IReadOnlyList<string>? tags)
    {
        if (tags == null) return true;
        return tags.Count == tags.Distinct(StringComparer.OrdinalIgnoreCase).Count();
    }
} 