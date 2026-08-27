using Application.Features.Football.Seasons.Queries;
using Domain.Entities.Football.Competitions;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class GetFootballSeasonContentBlocksByYearQueryValidator
    : AbstractValidator<GetFootballSeasonContentBlocksByYearQuery>
{
    public GetFootballSeasonContentBlocksByYearQueryValidator()
    {
        RuleFor(query => query.SeasonYear)
            .Must(year => FootballSeasonYear.TryParse(year, out _, out _))
            .When(query => !string.IsNullOrWhiteSpace(query.SeasonYear))
            .WithMessage("Season year must be a valid label such as 2025 or 2025-2026");
    }
}
