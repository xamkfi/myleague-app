using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="CancelHockeyTournamentCommand"/>.
/// </summary>
public class CancelHockeyTournamentCommandValidator : AbstractValidator<CancelHockeyTournamentCommand>
{
    public CancelHockeyTournamentCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
