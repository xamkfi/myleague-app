using Application.Queries.Common;
using Domain.Enums.Common;
using FluentValidation;

namespace Application.Validators.Queries.Common;

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