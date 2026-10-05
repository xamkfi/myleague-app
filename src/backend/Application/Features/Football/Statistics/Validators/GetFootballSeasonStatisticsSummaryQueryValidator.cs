using Application.Features.Football.Statistics.Queries;
using FluentValidation;

namespace Application.Features.Football.Statistics.Validators;

/// <summary>
/// Validator for GetFootballSeasonStatisticsSummaryQuery
/// </summary>
public class GetFootballSeasonStatisticsSummaryQueryValidator : AbstractValidator<GetFootballSeasonStatisticsSummaryQuery>
{
    public GetFootballSeasonStatisticsSummaryQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty().WithMessage("Competition ID is required");
        RuleFor(x => x.TopN).InclusiveBetween(1, 100).WithMessage("TopN must be between 1 and 100");
    }
}
