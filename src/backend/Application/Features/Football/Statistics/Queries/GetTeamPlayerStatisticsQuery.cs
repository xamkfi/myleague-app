using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving all player statistics for a specific team in a season
/// </summary>
public record GetTeamPlayerStatisticsQuery(Guid CompetitionId, Guid TeamId) : IRequest<Result<List<FootballPlayerSeasonStatisticsDto>>>;
