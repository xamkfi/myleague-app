using Application.Features.Common.Statistics;
using Application.Features.Football.Statistics.Queries;
using FluentValidation;

namespace Application.Features.Football.Statistics.Validators;

/// <summary>
/// Validator for <see cref="GetFootballAllTimePlayerStatisticsQuery"/>.
/// </summary>
public class GetFootballAllTimePlayerStatisticsQueryValidator : AbstractValidator<GetFootballAllTimePlayerStatisticsQuery>
{
    public GetFootballAllTimePlayerStatisticsQueryValidator()
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
            .Must(AllTimeStatSortRules.IsFootballSort)
            .WithMessage("Football all-time statistics can be sorted by games, goals, assists, points, yellow cards, or red cards.");
    }
}
