using Application.Features.Football.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class GetFootballSeasonByIdQueryValidator : AbstractValidator<GetFootballSeasonByIdQuery>
{
    public GetFootballSeasonByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Season ID is required");
    }
}
