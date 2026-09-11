using Application.Features.Hockey.Officials.Queries;
using Application.Services.Common;
using FluentValidation;

namespace Application.Features.Hockey.Officials.Validators;

/// <summary>
/// Validator for <see cref="GetPagedHockeyOfficialsQuery"/>.
/// </summary>
public class GetPagedHockeyOfficialsQueryValidator : AbstractValidator<GetPagedHockeyOfficialsQuery>
{
    private readonly IPaginationService _paginationService;

    public GetPagedHockeyOfficialsQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(pageSize => _paginationService.IsValidPageSize(GetPagedHockeyOfficialsQuery.ResourceKey, pageSize))
            .WithMessage(GetPageSizeErrorMessage());
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetPagedHockeyOfficialsQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }
}
