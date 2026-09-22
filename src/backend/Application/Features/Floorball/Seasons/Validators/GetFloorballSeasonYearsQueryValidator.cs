using Application.Features.Floorball.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

public class GetFloorballSeasonYearsQueryValidator : AbstractValidator<GetFloorballSeasonYearsQuery>
{
    public GetFloorballSeasonYearsQueryValidator()
    {
        RuleFor(query => query.TeamCategory!.Value)
            .IsInEnum()
            .When(query => query.TeamCategory.HasValue);
    }
}
