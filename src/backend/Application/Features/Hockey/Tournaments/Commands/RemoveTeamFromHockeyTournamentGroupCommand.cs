using Application.Common;
using Application.Features.Hockey.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Hockey.Tournaments.Commands;

/// <summary>
/// Command: RemoveTeamFromHockeyTournamentGroup.
/// </summary>
public record RemoveTeamFromHockeyTournamentGroupCommand(
    Guid TournamentId,
    Guid GroupId,
    Guid CompetitionTeamId) : IRequest<Result<HockeyTournamentDto>>;
