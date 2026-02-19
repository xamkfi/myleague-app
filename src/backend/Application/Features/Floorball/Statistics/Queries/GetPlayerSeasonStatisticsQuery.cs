using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Statistics;

/// <summary>
/// Query for retrieving player statistics for a specific season
/// </summary>
/// <param name="SeasonId">The season ID</param>
/// <param name="PlayerId">The player ID</param>
public record GetPlayerSeasonStatisticsQuery(Guid SeasonId, Guid PlayerId) : IRequest<Result<FloorballPlayerSeasonStatisticsDto>>;
