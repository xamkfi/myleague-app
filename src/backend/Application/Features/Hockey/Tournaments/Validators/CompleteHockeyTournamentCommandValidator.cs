using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="CompleteHockeyTournamentCommand"/>.
/// </summary>
public class CompleteHockeyTournamentCommandValidator : AbstractValidator<CompleteHockeyTournamentCommand>
{
    public CompleteHockeyTournamentCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
    }
}
