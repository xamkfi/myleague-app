using Application.Features.Common.Divisions.Queries;
using Application.Features.Common.Search.Queries;
using Application.Features.Common.MatchTimer.Queries;
using FluentValidation;
using System;

namespace Application.Features.Common.Divisions.Validators;

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
