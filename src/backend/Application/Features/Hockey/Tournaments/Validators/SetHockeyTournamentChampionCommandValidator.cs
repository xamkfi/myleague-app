using Application.Features.Hockey.Tournaments.Commands;
using FluentValidation;

namespace Application.Features.Hockey.Tournaments.Validators;

/// <summary>
/// Validator for <see cref="SetHockeyTournamentChampionCommand"/>.
/// </summary>
public class SetHockeyTournamentChampionCommandValidator : AbstractValidator<SetHockeyTournamentChampionCommand>
{
    public SetHockeyTournamentChampionCommandValidator()
    {
        RuleFor(x => x.TournamentId).NotEmpty().WithMessage("Tournament id is required.");
        RuleFor(x => x.ChampionCompetitionTeamId).NotEmpty().WithMessage("Champion competition team id is required.");
    }
}
