using Application.Features.Floorball.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

public class GetFloorballSeasonContentBlocksQueryValidator : AbstractValidator<GetFloorballSeasonContentBlocksQuery>
{
    public GetFloorballSeasonContentBlocksQueryValidator()
    {
        RuleFor(query => query.SeasonId)
            .NotEmpty()
            .WithMessage("Season ID is required");
    }
}
