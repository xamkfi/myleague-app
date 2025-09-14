using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Statistics;

/// <summary>
/// Query for retrieving detailed match statistics for both teams
/// </summary>
/// <param name="MatchId">The match ID</param>
public record GetMatchStatisticsQuery(Guid MatchId) : IRequest<Result<List<FloorballMatchTeamStatisticsDto>>>;
