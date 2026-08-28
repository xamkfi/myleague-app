using Application.Features.Floorball.Seasons.Queries;
using Domain.Entities.Floorball;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

public class GetFloorballSeasonContentBlocksByYearQueryValidator
    : AbstractValidator<GetFloorballSeasonContentBlocksByYearQuery>
{
    public GetFloorballSeasonContentBlocksByYearQueryValidator()
    {
        RuleFor(query => query.SeasonYear)
            .Must(year => FloorballSeasonYear.TryParse(year, out _, out _))
            .When(query => !string.IsNullOrWhiteSpace(query.SeasonYear))
            .WithMessage("Season year must be a valid label such as 2025 or 2025-2026");
    }
}
