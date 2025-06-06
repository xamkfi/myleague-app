using Application.Queries.NewsArticles;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Validators.Queries.NewsArticles;

/// <summary>
/// Validator for GetAllNewsArticlesQuery
/// </summary>
public class GetAllNewsArticlesQueryValidator : AbstractValidator<GetAllNewsArticlesQuery>
{
    public GetAllNewsArticlesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.Category)
            .Must(BeValidNewsCategory).WithMessage("Invalid news category")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.SportCategory)
            .Must(BeValidSportCategory).WithMessage("Invalid sport category")
            .When(x => !string.IsNullOrEmpty(x.SportCategory));

        RuleFor(x => x.Author)
            .MaximumLength(100).WithMessage("Author filter cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Author));
    }

    private static bool BeValidNewsCategory(string? category)
    {
        return !string.IsNullOrEmpty(category) && Enum.TryParse<NewsCategory>(category, true, out _);
    }

    private static bool BeValidSportCategory(string? category)
    {
        return !string.IsNullOrEmpty(category) && Enum.TryParse<SportsCategory>(category, true, out _);
    }
} 