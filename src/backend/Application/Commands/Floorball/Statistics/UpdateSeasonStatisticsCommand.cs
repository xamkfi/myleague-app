using Application.Common;
using MediatR;

namespace Application.Commands.Floorball.Statistics;

/// <summary>
/// Command for recalculating and updating all statistics for a season
/// </summary>
/// <param name="SeasonId">The season ID</param>
public record UpdateSeasonStatisticsCommand(Guid SeasonId) : IRequest<Result>;
