using Application.Features.Football.Players.Queries;
using FluentValidation;

namespace Application.Features.Football.Players.Validators;

/// <summary>
/// Validator for GetFootballPlayerByIdQuery
/// </summary>
public class GetFootballPlayerByIdQueryValidator : AbstractValidator<GetFootballPlayerByIdQuery>
{
    public GetFootballPlayerByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Player ID is required")
            .NotEqual(Guid.Empty).WithMessage("Player ID cannot be empty");
    }
} 
