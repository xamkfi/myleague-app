using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="ActivateHockeyTournamentCommand"/>.
/// </summary>
public class ActivateHockeyTournamentCommandValidator : AbstractValidator<ActivateHockeyTournamentCommand>
{
    public ActivateHockeyTournamentCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
