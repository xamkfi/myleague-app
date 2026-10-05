using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving comprehensive season statistics summary
/// </summary>
/// <param name="CompetitionId">The season or tournament ID</param>
/// <param name="TopN">Maximum rows in each player leaderboard (scorers, assists)</param>
public record GetFootballSeasonStatisticsSummaryQuery(Guid CompetitionId, int TopN = 10) : IRequest<Result<FootballSeasonStatisticsSummaryDto>>;
