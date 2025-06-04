using Application.Queries.Floorball.Match;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Match;

/// <summary>
/// Validator for GetFloorballMatchByIdQuery
/// </summary>
public class GetFloorballMatchByIdQueryValidator : AbstractValidator<GetFloorballMatchByIdQuery>
{
    public GetFloorballMatchByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Match ID is required")
            .NotEqual(Guid.Empty).WithMessage("Match ID cannot be empty");
    }
} 