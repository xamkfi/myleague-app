using Application.Features.Floorball.Teams.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Teams.Validators;

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
