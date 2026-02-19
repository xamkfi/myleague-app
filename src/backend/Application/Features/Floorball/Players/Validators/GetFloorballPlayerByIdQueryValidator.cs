using Application.Features.Floorball.Players.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Players.Validators;

/// <summary>
/// Validator for GetFloorballPlayerByIdQuery
/// </summary>
public class GetFloorballPlayerByIdQueryValidator : AbstractValidator<GetFloorballPlayerByIdQuery>
{
    public GetFloorballPlayerByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");
    }
} 
