using Application.Features.Hockey.Teams.Queries;
using Application.Services.Common;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="GetPagedHockeyTeamsQuery"/>.
/// </summary>
public class GetPagedHockeyTeamsQueryValidator : AbstractValidator<GetPagedHockeyTeamsQuery>
{
    private readonly IPaginationService _paginationService;

    public GetPagedHockeyTeamsQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(pageSize => _paginationService.IsValidPageSize(GetPagedHockeyTeamsQuery.ResourceKey, pageSize))
            .WithMessage(GetPageSizeErrorMessage());
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetPagedHockeyTeamsQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }
}
