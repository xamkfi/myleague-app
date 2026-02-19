using Application.Queries.Floorball.Player;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Player;

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