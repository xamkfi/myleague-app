using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Enums.Hockey.Competitions;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command to update hockey tournament rules.
/// </summary>
public record UpdateHockeyTournamentRulesCommand(
    Guid TournamentId,
    HockeyTournamentFormat Format,
    bool HasGroupStage,
    bool HasPlayoffs,
    bool HasBronzeGame,
    bool HasPlacementGames,
    int TeamsAdvancingPerGroup) : IRequest<Result<HockeyTournamentDto>>;
