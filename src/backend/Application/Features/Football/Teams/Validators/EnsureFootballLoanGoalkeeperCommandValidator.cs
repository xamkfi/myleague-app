using Application.Features.Football.Teams.Commands;
using FluentValidation;

namespace Application.Features.Football.Teams.Validators;

/// <summary>
/// Validator for <see cref="EnsureFootballLoanGoalkeeperCommand"/>.
/// </summary>
public class EnsureFootballLoanGoalkeeperCommandValidator : AbstractValidator<EnsureFootballLoanGoalkeeperCommand>
{
    public EnsureFootballLoanGoalkeeperCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required");

        RuleFor(x => x.CompetitionId)
            .NotEqual(Guid.Empty).WithMessage("Competition ID cannot be empty")
            .When(x => x.CompetitionId.HasValue);
    }
}
