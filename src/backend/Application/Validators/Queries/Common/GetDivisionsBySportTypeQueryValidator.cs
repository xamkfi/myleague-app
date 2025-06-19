using Application.Queries.Common;
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
            .NotEmpty().WithMessage("Sport type is required")
            .MaximumLength(50).WithMessage("Sport type cannot exceed 50 characters");
    }
} 