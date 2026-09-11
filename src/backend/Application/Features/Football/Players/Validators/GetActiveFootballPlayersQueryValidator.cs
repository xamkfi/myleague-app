using Application.Features.Football.Players.Queries;
using Application.Services.Common;
using Domain.Enums.Football;
using FluentValidation;

namespace Application.Features.Football.Players.Validators;

/// <summary>
/// Validator for GetActiveFootballPlayersQuery
/// </summary>
public class GetActiveFootballPlayersQueryValidator : AbstractValidator<GetActiveFootballPlayersQuery>
{
    private readonly IPaginationService _paginationService;

    public GetActiveFootballPlayersQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(BeValidPageSize).WithMessage(GetPageSizeErrorMessage());

        RuleFor(x => x.Position)
            .Must(BeValidPosition).WithMessage("Invalid football position")
            .When(x => !string.IsNullOrEmpty(x.Position));
    }

    private bool BeValidPageSize(int pageSize)
    {
        return _paginationService.IsValidPageSize(GetActiveFootballPlayersQuery.ResourceKey, pageSize);
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetActiveFootballPlayersQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }

    private static bool BeValidPosition(string? position)
    {
        return string.IsNullOrEmpty(position) || Enum.TryParse<FootballPosition>(position, true, out _);
    }
} 
