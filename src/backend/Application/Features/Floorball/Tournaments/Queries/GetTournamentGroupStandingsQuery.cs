using Application.Common;
using Application.Features.Floorball.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Floorball.Tournaments.Queries;

/// <summary>
/// Query for calculating and retrieving group standings for a specific tournament group
/// </summary>
/// <param name="TournamentId">The tournament ID</param>
/// <param name="GroupId">The group ID within the tournament</param>
public record GetTournamentGroupStandingsQuery(
    Guid TournamentId,
    Guid GroupId) : IRequest<Result<FloorballTournamentGroupStandingsDto>>;
