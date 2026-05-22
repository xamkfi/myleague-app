using Application.Common;
using Application.Features.Floorball.Teams.DTOs;
using MediatR;

namespace Application.Features.Floorball.Statistics.Queries;

/// <summary>
/// Query for retrieving a team's combined statistics aggregated across every competition the
/// team has played in (regular seasons + tournaments). Returned values represent the team's
/// "career" totals; consumers display this on the team page so tournament games and points
/// surface alongside the regular-season ones instead of being hidden behind a season-only filter.
/// </summary>
/// <param name="TeamId">The team whose stats should be aggregated.</param>
public record GetAggregatedTeamStatisticsQuery(Guid TeamId) : IRequest<Result<FloorballTeamSeasonStatisticsDto>>;
