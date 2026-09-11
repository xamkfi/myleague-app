using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving player statistics for a specific season
/// </summary>
public record GetPlayerSeasonStatisticsQuery(Guid CompetitionId, Guid PlayerId) : IRequest<Result<FootballPlayerSeasonStatisticsDto>>;
