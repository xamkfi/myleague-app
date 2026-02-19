using Application.Features.Floorball.Matches.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

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
