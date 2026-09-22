using Application.Features.Common.Organization.Divisions.Queries;
using Application.Features.Common.CrossCutting.Search.Queries;
using Application.Features.Common.CrossCutting.MatchTimer.Queries;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Features.Common.Organization.Divisions.Validators;

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
