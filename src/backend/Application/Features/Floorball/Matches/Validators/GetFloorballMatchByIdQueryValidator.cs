using Application.Features.Floorball.Matches.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

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
