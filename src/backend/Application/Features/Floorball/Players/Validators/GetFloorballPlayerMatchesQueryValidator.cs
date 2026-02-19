using Application.Queries.Floorball.Player;
using FluentValidation;

namespace Application.Validators.Queries.Floorball.Players
{
    /// <summary>
    /// Validator for GetFloorballPlayerMatchesQuery
    /// </summary>
    public class GetFloorballPlayerMatchesQueryValidator : AbstractValidator<GetFloorballPlayerMatchesQuery>
    {
        public GetFloorballPlayerMatchesQueryValidator()
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