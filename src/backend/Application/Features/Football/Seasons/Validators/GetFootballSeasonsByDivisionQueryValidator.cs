using Application.Features.Football.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Football.Seasons.Validators;

public class GetFootballSeasonsByDivisionQueryValidator : AbstractValidator<GetFootballSeasonsByDivisionQuery>
{
    public GetFootballSeasonsByDivisionQueryValidator()
    {
        RuleFor(x => x.DivisionId).NotEmpty().WithMessage("Division ID is required");
    }
}
