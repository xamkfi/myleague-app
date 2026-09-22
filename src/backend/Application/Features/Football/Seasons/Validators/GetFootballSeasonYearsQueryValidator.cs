using Application.Features.Football.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class GetFootballSeasonYearsQueryValidator : AbstractValidator<GetFootballSeasonYearsQuery>
{
    public GetFootballSeasonYearsQueryValidator()
    {
        RuleFor(query => query.TeamCategory!.Value)
            .IsInEnum()
            .When(query => query.TeamCategory.HasValue);
    }
}
