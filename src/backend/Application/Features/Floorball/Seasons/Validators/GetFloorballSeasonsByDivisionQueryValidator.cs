using Application.Queries.Floorball.Season;
using Domain.Enums.Floorball;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Season;

/// <summary>
/// Validator for GetFloorballSeasonsByDivisionQuery
/// </summary>
public class GetFloorballSeasonsByDivisionQueryValidator : AbstractValidator<GetFloorballSeasonsByDivisionQuery>
{
    public GetFloorballSeasonsByDivisionQueryValidator()
    {
        RuleFor(x => x.DivisionId)
             .NotNull().WithMessage("Division is required");
    }
} 
