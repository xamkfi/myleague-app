using Application.Features.Football.Statistics.Queries;
using FluentValidation;

namespace Application.Features.Football.Statistics.Validators;

/// <summary>
/// Validator for <see cref="GetFootballAllTimeTeamsQuery"/>.
/// </summary>
public class GetFootballAllTimeTeamsQueryValidator : AbstractValidator<GetFootballAllTimeTeamsQuery>
{
    public GetFootballAllTimeTeamsQueryValidator()
    {
        RuleFor(x => x.TeamCategory).IsInEnum();
        RuleFor(x => x.CompetitionType).IsInEnum();
    }
}
