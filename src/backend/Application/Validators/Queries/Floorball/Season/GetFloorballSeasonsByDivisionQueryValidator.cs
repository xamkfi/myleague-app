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
        RuleFor(x => x.Division)
            .NotNull().WithMessage("Division is required")
            .IsInEnum().WithMessage("Invalid division value");
    }
} 