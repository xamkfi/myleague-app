using Application.Features.Hockey.Matches.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class GetHockeyMatchByIdQueryValidator : AbstractValidator<GetHockeyMatchByIdQuery>
{
    public GetHockeyMatchByIdQueryValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
    }
}
