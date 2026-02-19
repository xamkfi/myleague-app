using Application.Features.Floorball.Seasons.Queries;
using Domain.Enums.Floorball;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

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
