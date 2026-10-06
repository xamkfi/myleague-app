using Application.Features.Common.Statistics;
using Application.Features.Hockey.Statistics.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Statistics.Validators;

/// <summary>
/// Validator for <see cref="GetHockeyAllTimePlayerStatisticsQuery"/>.
/// </summary>
public class GetHockeyAllTimePlayerStatisticsQueryValidator : AbstractValidator<GetHockeyAllTimePlayerStatisticsQuery>
{
    public GetHockeyAllTimePlayerStatisticsQueryValidator()
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
            .WithMessage("Hockey all-time statistics can be sorted by games, goals, assists, points, or penalty minutes.");
    }
}
