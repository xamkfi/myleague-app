using Application.Common;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Statistics.Queries;

/// <summary>
/// Query for retrieving all goalie statistics for a specific team in a competition
/// </summary>
/// <param name="CompetitionId">The season or tournament ID</param>
/// <param name="TeamId">The team ID</param>
public record GetFloorballTeamGoalieStatisticsQuery(Guid CompetitionId, Guid TeamId) : IRequest<Result<List<FloorballGoalieSeasonStatisticsDto>>>;
