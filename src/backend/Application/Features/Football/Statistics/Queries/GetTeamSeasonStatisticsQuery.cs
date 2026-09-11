using Application.Common;
using Application.Features.Football.Teams.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving team statistics for a specific season
/// </summary>
public record GetTeamSeasonStatisticsQuery(Guid CompetitionId, Guid TeamId) : IRequest<Result<FootballTeamSeasonStatisticsDto>>;
