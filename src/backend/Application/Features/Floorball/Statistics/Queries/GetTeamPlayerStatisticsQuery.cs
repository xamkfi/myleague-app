using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Statistics.Queries;

/// <summary>
/// Query for retrieving all player statistics for a specific team in a season
/// </summary>
/// <param name="SeasonId">The season ID</param>
/// <param name="TeamId">The team ID</param>
public record GetTeamPlayerStatisticsQuery(Guid CompetitionId, Guid TeamId) : IRequest<Result<List<FloorballPlayerSeasonStatisticsDto>>>;
