using Application.Features.Football.Teams.Queries;
using FluentValidation;

namespace Application.Features.Football.Teams.Validators;

/// <summary>
/// Validator for GetFootballTeamByIdQuery
/// </summary>
public class GetFootballTeamByIdQueryValidator : AbstractValidator<GetFootballTeamByIdQuery>
{
    public GetFootballTeamByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Team ID is required")
            .NotEqual(Guid.Empty).WithMessage("Team ID cannot be empty");
    }
} 
