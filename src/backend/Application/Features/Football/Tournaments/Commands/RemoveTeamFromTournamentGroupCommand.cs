using Application.Common;
using Application.Features.Football.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Football.Tournaments.Commands;

/// <summary>
/// Command to remove a team from a tournament group
/// </summary>
public record RemoveTeamFromTournamentGroupCommand(
    Guid CompetitionId,
    Guid GroupId,
    Guid TeamId) : IRequest<Result<FootballTournamentDto>>;
