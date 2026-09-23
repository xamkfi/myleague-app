using Application.Features.Floorball.Seasons.Queries;
using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
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
