using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Statistics.Queries;

/// <summary>
/// Query for retrieving per-player statistics for a team aggregated across every competition the
/// team has played in. Each player ends up with a single row that sums their stats across the
/// regular season, tournaments and any other competition the team participated in, so the team
/// page's player table shows total goals/points/games — not just the regular-season slice.
/// </summary>
/// <param name="TeamId">The team whose player statistics should be aggregated.</param>
public record GetAggregatedTeamPlayerStatisticsQuery(Guid TeamId) : IRequest<Result<List<FloorballPlayerSeasonStatisticsDto>>>;
