using Application.Features.Floorball.Seasons.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Seasons.Validators;

/// <summary>
/// Validator for GetFloorballSeasonByIdQuery
/// </summary>
public class GetFloorballSeasonByIdQueryValidator : AbstractValidator<GetFloorballSeasonByIdQuery>
{
    public GetFloorballSeasonByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Season ID is required")
            .NotEqual(Guid.Empty).WithMessage("Season ID cannot be empty");
    }
} 
