using Application.Features.Floorball.Teams.Commands;
using FluentValidation;

namespace Application.Features.Floorball.Teams.Validators;

/// <summary>
/// Validator for <see cref="EnsureFloorballLoanPlayersCommand"/>.
/// </summary>
public class EnsureFloorballLoanPlayersCommandValidator : AbstractValidator<EnsureFloorballLoanPlayersCommand>
{
    public EnsureFloorballLoanPlayersCommandValidator()
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
