using Application.Features.Football.Players.Queries;
using FluentValidation;

namespace Application.Features.Football.Players.Validators
{
    /// <summary>
    /// Validator for GetFootballPlayerMatchesQuery
    /// </summary>
    public class GetFootballPlayerMatchesQueryValidator : AbstractValidator<GetFootballPlayerMatchesQuery>
    {
        public GetFootballPlayerMatchesQueryValidator()
        {
            RuleFor(x => x.PlayerId)
                .NotEmpty()
                .WithMessage("Player ID is required");

            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .WithMessage("Limit must be greater than 0")
                .LessThanOrEqualTo(50)
                .WithMessage("Limit cannot exceed 50");
        }
    }
} 
