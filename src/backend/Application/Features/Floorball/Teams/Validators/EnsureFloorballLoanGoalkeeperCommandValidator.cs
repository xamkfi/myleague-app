using Application.Features.Floorball.Teams.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Teams.Validators;

/// <summary>
/// Validator for <see cref="EnsureFloorballLoanGoalkeeperCommand"/>.
/// </summary>
public class EnsureFloorballLoanGoalkeeperCommandValidator : AbstractValidator<EnsureFloorballLoanGoalkeeperCommand>
{
    public EnsureFloorballLoanGoalkeeperCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required");

        RuleFor(x => x.CompetitionId)
            .NotEqual(Guid.Empty).WithMessage("Competition ID cannot be empty")
            .When(x => x.CompetitionId.HasValue);
    }
}
