using Application.Features.Common.Divisions.Queries;
using Application.Features.Common.Search.Queries;
using Application.Features.Common.MatchTimer.Queries;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Features.Common.Divisions.Validators;

/// <summary>
/// Validator for GetDivisionsBySportTypeQuery
/// </summary>
public class GetDivisionsBySportTypeQueryValidator : AbstractValidator<GetDivisionsBySportTypeQuery>
{
    public GetDivisionsBySportTypeQueryValidator()
    {
        RuleFor(x => x.SportType)
            .IsInEnum().WithMessage("Sport type is invalid")
            .Must(st => st != SportsCategory.None).WithMessage("Sport type is required");
    }
} 
