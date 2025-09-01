using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Statistics;

/// <summary>
/// Query for retrieving top scorers for a specific season
/// </summary>
/// <param name="SeasonId">The season ID</param>
/// <param name="TopN">Number of top scorers to return (default: 10)</param>
public record GetTopScorersQuery(Guid SeasonId, int TopN = 10) : IRequest<Result<List<FloorballPlayerSeasonStatisticsDto>>>;
