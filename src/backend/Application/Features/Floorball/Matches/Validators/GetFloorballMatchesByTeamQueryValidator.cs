using Application.Features.Floorball.Matches.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

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
