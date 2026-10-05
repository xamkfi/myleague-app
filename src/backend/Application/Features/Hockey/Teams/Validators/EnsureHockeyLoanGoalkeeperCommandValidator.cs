using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="EnsureHockeyLoanGoalkeeperCommand"/>.
/// </summary>
public class EnsureHockeyLoanGoalkeeperCommandValidator : AbstractValidator<EnsureHockeyLoanGoalkeeperCommand>
{
    public EnsureHockeyLoanGoalkeeperCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required");

        RuleFor(x => x.CompetitionId)
            .NotEqual(Guid.Empty).WithMessage("Competition ID cannot be empty")
            .When(x => x.CompetitionId.HasValue);
    }
}
