using Application.Queries.Floorball.Match;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Match;

/// <summary>
/// Validator for GetFloorballMatchesBySeasonQuery
/// </summary>
public class GetFloorballMatchesBySeasonQueryValidator : AbstractValidator<GetFloorballMatchesBySeasonQuery>
{
    public GetFloorballMatchesBySeasonQueryValidator()
    {
        RuleFor(x => x.SeasonId)
            .NotEmpty().WithMessage("Season ID is required")
            .NotEqual(Guid.Empty).WithMessage("Season ID cannot be empty");
    }
} 