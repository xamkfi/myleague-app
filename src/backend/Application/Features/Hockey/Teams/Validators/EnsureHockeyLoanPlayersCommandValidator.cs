using Application.Features.Hockey.Teams.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Teams.Validators;

/// <summary>
/// Validator for <see cref="EnsureHockeyLoanPlayersCommand"/>.
/// </summary>
public class EnsureHockeyLoanPlayersCommandValidator : AbstractValidator<EnsureHockeyLoanPlayersCommand>
{
    public EnsureHockeyLoanPlayersCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required");

        RuleFor(x => x.CompetitionId)
            .NotEqual(Guid.Empty).WithMessage("Competition ID cannot be empty")
            .When(x => x.CompetitionId.HasValue);

        RuleFor(x => x.Count)
            .InclusiveBetween(1, 20).WithMessage("Loan player count must be between 1 and 20");
    }
}
