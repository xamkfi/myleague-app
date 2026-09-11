using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving per-player statistics for a team aggregated across every competition the
/// team has played in.
/// </summary>
public record GetAggregatedTeamPlayerStatisticsQuery(Guid TeamId) : IRequest<Result<List<FootballPlayerSeasonStatisticsDto>>>;
