using Application.Queries.Floorball.Match;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Match;

/// <summary>
/// Validator for GetFloorballMatchesByTeamQuery
/// </summary>
public class GetFloorballMatchesByTeamQueryValidator : AbstractValidator<GetFloorballMatchesByTeamQuery>
{
    public GetFloorballMatchesByTeamQueryValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
    }
} 