using Application.Features.Common.SeasonContentBlocks.Queries;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Features.Common.SeasonContentBlocks.Validators;

/// <summary>
/// Validator for GetAllSeasonContentBlocksQuery
/// </summary>
public class GetAllSeasonContentBlocksQueryValidator : AbstractValidator<GetAllSeasonContentBlocksQuery>
{
    public GetAllSeasonContentBlocksQueryValidator()
    {
        RuleFor(x => x)
            .Must(HasCompetitionFilterOrSportYearFilter)
            .WithMessage("Provide a competition ID, or both a sport and a season year.");

        When(x => !x.CompetitionId.HasValue || x.CompetitionId.Value == Guid.Empty, () =>
        {
            RuleFor(x => x.Sport)
                .NotNull().WithMessage("Sport is required when filtering by season year")
                .Must(sport => sport is SportsCategory.Floorball or SportsCategory.Football or SportsCategory.Icehockey)
                .WithMessage("Sport must be Floorball, Football, or Icehockey");

            RuleFor(x => x.SeasonYear)
                .NotEmpty().WithMessage("Season year is required when filtering by sport");
        });
    }

    private static bool HasCompetitionFilterOrSportYearFilter(GetAllSeasonContentBlocksQuery query)
    {
        if (query.CompetitionId.HasValue && query.CompetitionId.Value != Guid.Empty)
        {
            return true;
        }

        return query.Sport.HasValue && !string.IsNullOrWhiteSpace(query.SeasonYear);
    }
}
