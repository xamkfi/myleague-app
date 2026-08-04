using Application.Features.Hockey.Competitions.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Competitions.Validators;

public class GetEffectiveHockeyCompetitionRulesQueryValidator
    : AbstractValidator<GetEffectiveHockeyCompetitionRulesQuery>
{
    public GetEffectiveHockeyCompetitionRulesQueryValidator()
    {
        RuleFor(x => x.CompetitionId).NotEmpty();
    }
}
