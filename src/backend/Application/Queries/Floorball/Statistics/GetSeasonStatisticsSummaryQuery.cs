using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Statistics;

/// <summary>
/// Query for retrieving comprehensive season statistics summary
/// </summary>
/// <param name="SeasonId">The season ID</param>
public record GetSeasonStatisticsSummaryQuery(Guid SeasonId) : IRequest<Result<FloorballSeasonStatisticsSummaryDto>>;
