using Application.Features.Football.Teams.Commands;
using FluentValidation;

namespace Application.Features.Football.Teams.Validators;

/// <summary>
/// Validator for <see cref="EnsureFootballLoanPlayersCommand"/>.
/// </summary>
public class EnsureFootballLoanPlayersCommandValidator : AbstractValidator<EnsureFootballLoanPlayersCommand>
{
    public EnsureFootballLoanPlayersCommandValidator()
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
