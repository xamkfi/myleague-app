using Application.Queries.Floorball.Team;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Team;

/// <summary>
/// Validator for GetFloorballTeamByIdQuery
/// </summary>
public class GetFloorballTeamByIdQueryValidator : AbstractValidator<GetFloorballTeamByIdQuery>
{
    public GetFloorballTeamByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
    }
} 