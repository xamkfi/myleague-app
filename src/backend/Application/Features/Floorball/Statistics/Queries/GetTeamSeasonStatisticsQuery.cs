using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Statistics;

/// <summary>
/// Query for retrieving team statistics for a specific season
/// </summary>
/// <param name="SeasonId">The season ID</param>
/// <param name="TeamId">The team ID</param>
public record GetTeamSeasonStatisticsQuery(Guid SeasonId, Guid TeamId) : IRequest<Result<FloorballTeamSeasonStatisticsDto>>;
