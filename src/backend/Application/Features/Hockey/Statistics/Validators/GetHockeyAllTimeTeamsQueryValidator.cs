using Application.Features.Hockey.Statistics.Queries;
using FluentValidation;

namespace Application.Features.Hockey.Statistics.Validators;

/// <summary>
/// Validator for <see cref="GetHockeyAllTimeTeamsQuery"/>.
/// </summary>
public class GetHockeyAllTimeTeamsQueryValidator : AbstractValidator<GetHockeyAllTimeTeamsQuery>
{
    public GetHockeyAllTimeTeamsQueryValidator()
    {
        RuleFor(x => x.TeamCategory).IsInEnum();
        RuleFor(x => x.CompetitionType).IsInEnum();
    }
}
