using Application.Features.Common.Statistics;
using Application.Features.Floorball.Statistics.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Statistics.Validators;

/// <summary>
/// Validator for <see cref="GetFloorballAllTimePlayerStatisticsQuery"/>.
/// </summary>
public class GetFloorballAllTimePlayerStatisticsQueryValidator : AbstractValidator<GetFloorballAllTimePlayerStatisticsQuery>
{
    public GetFloorballAllTimePlayerStatisticsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.TeamCategory).IsInEnum();
        RuleFor(x => x.CompetitionType).IsInEnum();
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.TeamId).NotEqual(Guid.Empty).When(x => x.TeamId.HasValue);
        RuleFor(x => x.Sort)
            .IsInEnum()
            .Must(AllTimeStatSortRules.IsSkaterSort)
            .WithMessage("Floorball all-time statistics can be sorted by games, goals, assists, points, or penalty minutes.");
    }
}
