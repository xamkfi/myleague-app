using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="DeactivateHockeyTournamentCommand"/>.
/// </summary>
public class DeactivateHockeyTournamentCommandValidator : AbstractValidator<DeactivateHockeyTournamentCommand>
{
    public DeactivateHockeyTournamentCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
