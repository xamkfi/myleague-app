using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving detailed match statistics for both teams
/// </summary>
public record GetMatchStatisticsQuery(Guid MatchId) : IRequest<Result<List<FootballMatchTeamStatisticsDto>>>;
