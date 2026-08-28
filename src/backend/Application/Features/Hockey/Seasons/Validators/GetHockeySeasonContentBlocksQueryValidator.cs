using Application.Features.Hockey.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Seasons.Validators;

public class GetHockeySeasonContentBlocksQueryValidator : AbstractValidator<GetHockeySeasonContentBlocksQuery>
{
    public GetHockeySeasonContentBlocksQueryValidator()
    {
        RuleFor(query => query.SeasonId)
            .NotEmpty()
            .WithMessage("Season ID is required");
    }
}
