using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving top scorers for a specific season
/// </summary>
public record GetTopScorersQuery(Guid CompetitionId, int TopN = 10) : IRequest<Result<List<FootballPlayerSeasonStatisticsDto>>>;
