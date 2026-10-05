using Application.Features.Floorball.Statistics.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Statistics.Validators;

/// <summary>
/// Validator for GetFloorballSeasonStatisticsSummaryQuery
/// </summary>
public class GetFloorballSeasonStatisticsSummaryQueryValidator : AbstractValidator<GetFloorballSeasonStatisticsSummaryQuery>
{
    public GetFloorballSeasonStatisticsSummaryQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty().WithMessage("Competition ID is required");
        RuleFor(x => x.TopN).InclusiveBetween(1, 100).WithMessage("TopN must be between 1 and 100");
    }
}
