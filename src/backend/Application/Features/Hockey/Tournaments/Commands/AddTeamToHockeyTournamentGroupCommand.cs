using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command to add a competition team to a hockey tournament group.
/// The team must already belong to the tournament as a competition team.
/// </summary>
/// <param name="TournamentId">Tournament id</param>
/// <param name="GroupId">Tournament group id</param>
/// <param name="CompetitionTeamId">Competition-team membership id</param>
/// <param name="Seed">Optional seed within the group</param>
public record AddTeamToHockeyTournamentGroupCommand(
    Guid TournamentId,
    Guid GroupId,
    Guid CompetitionTeamId,
    int? Seed = null) : IRequest<Result<HockeyTournamentDto>>;
