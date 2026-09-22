using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving comprehensive season statistics summary
/// </summary>
public record GetFootballSeasonStatisticsSummaryQuery(Guid CompetitionId) : IRequest<Result<FootballSeasonStatisticsSummaryDto>>;
