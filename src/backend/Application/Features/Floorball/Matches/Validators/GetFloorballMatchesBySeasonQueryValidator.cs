using Application.Features.Floorball.Matches.Queries;
using FluentValidation;

namespace Application.Features.Floorball.Matches.Validators;

/// <summary>
/// Validator for GetFloorballMatchesBySeasonQuery
/// </summary>
public class GetFloorballMatchesBySeasonQueryValidator : AbstractValidator<GetFloorballMatchesBySeasonQuery>
{
    public GetFloorballMatchesBySeasonQueryValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty().WithMessage("Competition ID is required")
            .NotEqual(Guid.Empty).WithMessage("Competition ID cannot be empty");
    }
} 
