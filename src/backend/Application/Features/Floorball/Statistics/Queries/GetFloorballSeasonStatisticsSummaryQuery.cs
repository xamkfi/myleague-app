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
/// Query for retrieving comprehensive season statistics summary
/// </summary>
/// <param name="CompetitionId">The season or tournament ID</param>
/// <param name="TopN">Maximum rows in each player leaderboard (scorers, assists, goalies)</param>
public record GetFloorballSeasonStatisticsSummaryQuery(Guid CompetitionId, int TopN = 10) : IRequest<Result<FloorballSeasonStatisticsSummaryDto>>;
