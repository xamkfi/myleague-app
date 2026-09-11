using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Commands;

/// <summary>
/// Command to remove a team from a tournament group
/// </summary>
public record RemoveTeamFromTournamentGroupCommand(
    Guid CompetitionId,
    Guid GroupId,
    Guid TeamId) : IRequest<Result<FloorballTournamentDto>>;
