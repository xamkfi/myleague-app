using Application.Queries.Common;
using FluentValidation;
using System;

namespace Application.Validators.Queries.Common;

/// <summary>
/// Validator for GetDivisionByIdQuery
/// </summary>
public class GetDivisionByIdQueryValidator : AbstractValidator<GetDivisionByIdQuery>
{
    public GetDivisionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Division ID is required")
            .NotEqual(Guid.Empty).WithMessage("Division ID cannot be empty");
    }
} 