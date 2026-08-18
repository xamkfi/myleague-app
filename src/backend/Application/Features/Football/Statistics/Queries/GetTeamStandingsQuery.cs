using Application.Common;
using Application.Features.Football.Teams.DTOs;
using MediatR;

namespace Application.Features.Football.Statistics.Queries;

/// <summary>
/// Query for retrieving team standings for a specific season
/// </summary>
public record GetTeamStandingsQuery(Guid CompetitionId) : IRequest<Result<List<FootballTeamSeasonStatisticsDto>>>;
