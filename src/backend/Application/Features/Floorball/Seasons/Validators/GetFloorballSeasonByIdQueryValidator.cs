using Application.Queries.Floorball.Season;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Season;

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