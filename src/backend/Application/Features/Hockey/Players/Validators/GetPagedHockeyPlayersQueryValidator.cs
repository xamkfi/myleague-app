using Application.Features.Hockey.Players.Queries;
using Application.Services.Common;
using FluentValidation;

namespace Application.Features.Hockey.Players.Validators;

/// <summary>
/// Validator for <see cref="GetPagedHockeyPlayersQuery"/>.
/// </summary>
public class GetPagedHockeyPlayersQueryValidator : AbstractValidator<GetPagedHockeyPlayersQuery>
{
    private readonly IPaginationService _paginationService;

    public GetPagedHockeyPlayersQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(pageSize => _paginationService.IsValidPageSize(GetPagedHockeyPlayersQuery.ResourceKey, pageSize))
            .WithMessage(GetPageSizeErrorMessage());
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetPagedHockeyPlayersQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }
}
