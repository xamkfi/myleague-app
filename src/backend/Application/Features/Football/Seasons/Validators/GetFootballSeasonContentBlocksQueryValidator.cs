using Application.Features.Football.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class GetFootballSeasonContentBlocksQueryValidator : AbstractValidator<GetFootballSeasonContentBlocksQuery>
{
    public GetFootballSeasonContentBlocksQueryValidator()
    {
        RuleFor(query => query.SeasonId)
            .NotEmpty()
            .WithMessage("Season ID is required");
    }
}
