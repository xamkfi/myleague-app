using Application.Features.Football.Matches.Queries;
using Application.Services.Common;
using FluentValidation;

namespace Application.Features.Football.Matches.Validators;

public class GetAllFootballMatchesQueryValidator : AbstractValidator<GetAllFootballMatchesQuery>
{
    private readonly IPaginationService _paginationService;

    public GetAllFootballMatchesQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(BeValidPageSize).WithMessage(GetPageSizeErrorMessage());

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be less than or equal to end date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be greater than or equal to start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }

    private bool BeValidPageSize(int pageSize) =>
        _paginationService.IsValidPageSize(GetAllFootballMatchesQuery.ResourceKey, pageSize);

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetAllFootballMatchesQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }
}
