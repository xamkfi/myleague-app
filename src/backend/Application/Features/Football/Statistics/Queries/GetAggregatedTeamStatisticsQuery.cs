using Application.Common;
using Application.Features.Football.Teams.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving a team's combined statistics aggregated across every competition the
/// team has played in (regular seasons + tournaments).
/// </summary>
public record GetAggregatedTeamStatisticsQuery(Guid TeamId) : IRequest<Result<FootballTeamSeasonStatisticsDto>>;
