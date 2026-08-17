using Application.Features.Hockey.Matches.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Matches.Validators;

public class GetHockeyMatchesByCompetitionQueryValidator
    : AbstractValidator<GetHockeyMatchesByCompetitionQuery>
{
    public GetHockeyMatchesByCompetitionQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
    }
}

public class GetHockeyMatchesByTeamQueryValidator : AbstractValidator<GetHockeyMatchesByTeamQuery>
{
    public GetHockeyMatchesByTeamQueryValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
    }
}
