using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: SetHockeyTournamentChampion.
/// </summary>
public record SetHockeyTournamentChampionCommand(
    Guid TournamentId,
    Guid ChampionCompetitionTeamId) : IRequest<Result<HockeyTournamentDto>>;
