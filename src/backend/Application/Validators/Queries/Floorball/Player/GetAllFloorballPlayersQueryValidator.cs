using Application.Queries.Floorball.Player;
using Application.Services.Common;
using Domain.Enums.Floorball;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Player;

/// <summary>
/// Validator for GetAllFloorballPlayersQuery
/// </summary>
public class GetAllFloorballPlayersQueryValidator : AbstractValidator<GetAllFloorballPlayersQuery>
{
    private readonly IPaginationService _paginationService;

    public GetAllFloorballPlayersQueryValidator(IPaginationService paginationService)
    {
        _paginationService = paginationService;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .Must(BeValidPageSize).WithMessage(GetPageSizeErrorMessage());

        RuleFor(x => x.Position)
            .Must(BeValidPosition).WithMessage("Invalid floorball position")
            .When(x => !string.IsNullOrEmpty(x.Position));

        RuleFor(x => x.SearchTerm)
            .MinimumLength(2).WithMessage("Search term must be at least 2 characters")
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));
    }

    private bool BeValidPageSize(int pageSize)
    {
        return _paginationService.IsValidPageSize(GetAllFloorballPlayersQuery.ResourceKey, pageSize);
    }

    private string GetPageSizeErrorMessage()
    {
        PaginationSettings settings = _paginationService.GetPaginationSettings(GetAllFloorballPlayersQuery.ResourceKey);
        return $"Page size must be 0 (use default) or between {settings.MinPageSize} and {settings.MaxPageSize}";
    }

    private static bool BeValidPosition(string? position)
    {
        return string.IsNullOrEmpty(position) || Enum.TryParse<FloorballPosition>(position, true, out _);
    }
} 