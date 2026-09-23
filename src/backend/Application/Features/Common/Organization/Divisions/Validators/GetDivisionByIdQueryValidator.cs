using Application.Features.Common.Organization.Divisions.Queries;
using Application.Features.Common.CrossCutting.Search.Queries;
using Application.Features.Common.CrossCutting.MatchTimer.Queries;
using FluentValidation;
using System;

namespace Application.Features.Common.Organization.Divisions.Validators;

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
