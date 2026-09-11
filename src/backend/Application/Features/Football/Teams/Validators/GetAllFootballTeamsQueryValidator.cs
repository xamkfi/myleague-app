using Application.Features.Football.Teams.Queries;
using Application.Services.Common;
using FluentValidation;

namespace Application.Features.Football.Teams.Validators;

/// <summary>
/// Validator for GetAllFootballTeamsQuery
/// </summary>
public class GetAllFootballTeamsQueryValidator : AbstractValidator<GetAllFootballTeamsQuery>
{
    private readonly IPaginationService _paginationService;

    public GetAllFootballTeamsQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(BeValidPageSize).WithMessage(GetPageSizeErrorMessage());

        RuleFor(x => x.Division)
            .MaximumLength(50).WithMessage("Division filter cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Division));
    }

    private bool BeValidPageSize(int pageSize)
    {
        return _paginationService.IsValidPageSize(GetAllFootballTeamsQuery.ResourceKey, pageSize);
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetAllFootballTeamsQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }
} 
