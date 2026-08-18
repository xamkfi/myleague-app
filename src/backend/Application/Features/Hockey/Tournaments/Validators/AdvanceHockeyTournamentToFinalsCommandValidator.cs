using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="AdvanceHockeyTournamentToFinalsCommand"/>.
/// </summary>
public class AdvanceHockeyTournamentToFinalsCommandValidator : AbstractValidator<AdvanceHockeyTournamentToFinalsCommand>
{
    public AdvanceHockeyTournamentToFinalsCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
