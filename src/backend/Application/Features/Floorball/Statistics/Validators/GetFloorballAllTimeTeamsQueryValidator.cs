using Application.Features.Floorball.Statistics.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Statistics.Validators;

/// <summary>
/// Validator for <see cref="GetFloorballAllTimeTeamsQuery"/>.
/// </summary>
public class GetFloorballAllTimeTeamsQueryValidator : AbstractValidator<GetFloorballAllTimeTeamsQuery>
{
    public GetFloorballAllTimeTeamsQueryValidator()
    {
        RuleFor(x => x.TeamCategory).IsInEnum();
        RuleFor(x => x.CompetitionType).IsInEnum();
    }
}
