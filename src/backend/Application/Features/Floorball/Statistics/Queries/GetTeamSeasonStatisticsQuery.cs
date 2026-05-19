using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.Statistics.Queries;

/// <summary>
/// Query for retrieving team statistics for a specific season
/// </summary>
/// <param name="SeasonId">The season ID</param>
/// <param name="TeamId">The team ID</param>
public record GetTeamSeasonStatisticsQuery(Guid CompetitionId, Guid TeamId) : IRequest<Result<FloorballTeamSeasonStatisticsDto>>;
