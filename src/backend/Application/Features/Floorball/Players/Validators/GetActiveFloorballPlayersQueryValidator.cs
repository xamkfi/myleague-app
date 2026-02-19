using Application.Features.Floorball.Players.Queries;
using Application.Services.Common;
using Domain.Enums.Floorball;
using FluentValidation;

namespace Application.Features.Floorball.Players.Validators;

/// <summary>
/// Validator for GetActiveFloorballPlayersQuery
/// </summary>
public class GetActiveFloorballPlayersQueryValidator : AbstractValidator<GetActiveFloorballPlayersQuery>
{
    private readonly IPaginationService _paginationService;

    public GetActiveFloorballPlayersQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(BeValidPageSize).WithMessage(GetPageSizeErrorMessage());

        RuleFor(x => x.Position)
            .Must(BeValidPosition).WithMessage("Invalid floorball position")
            .When(x => !string.IsNullOrEmpty(x.Position));
    }

    private bool BeValidPageSize(int pageSize)
    {
        return _paginationService.IsValidPageSize(GetActiveFloorballPlayersQuery.ResourceKey, pageSize);
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetActiveFloorballPlayersQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }

    private static bool BeValidPosition(string? position)
    {
        return string.IsNullOrEmpty(position) || Enum.TryParse<FloorballPosition>(position, true, out _);
    }
} 
