using Application.Features.Hockey.Matches.Queries;
using Application.Services.Common;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class GetHockeyMatchesByCompetitionQueryValidator
    : AbstractValidator<GetHockeyMatchesByCompetitionQuery>
{
    public GetHockeyMatchesByCompetitionQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
    }
}

public class GetHockeyMatchesByTeamQueryValidator : AbstractValidator<GetHockeyMatchesByTeamQuery>
{
    public GetHockeyMatchesByTeamQueryValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
    }
}

public class GetPagedHockeyMatchesQueryValidator : AbstractValidator<GetPagedHockeyMatchesQuery>
{
    private readonly IPaginationService _paginationService;

    public GetPagedHockeyMatchesQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(pageSize => _paginationService.IsValidPageSize(GetPagedHockeyMatchesQuery.ResourceKey, pageSize))
            .WithMessage(GetPageSizeErrorMessage());

        RuleFor(x => x.SortOrder)
            .Must(order => string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort order must be asc or desc.");
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetPagedHockeyMatchesQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }
}
