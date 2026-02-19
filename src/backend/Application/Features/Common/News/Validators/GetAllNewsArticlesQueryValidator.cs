using Application.Features.Common.News.Queries;
using Application.Services.Common;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Features.Common.News.Validators;

/// <summary>
/// Validator for GetAllNewsArticlesQuery
/// </summary>
public class GetAllNewsArticlesQueryValidator : AbstractValidator<GetAllNewsArticlesQuery>
{
    private readonly IPaginationService _paginationService;

    public GetAllNewsArticlesQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(BeValidPageSize).WithMessage(GetPageSizeErrorMessage());

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

    private bool BeValidPageSize(int pageSize)
    {
        return _paginationService.IsValidPageSize(GetAllNewsArticlesQuery.ResourceKey, pageSize);
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetAllNewsArticlesQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
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
