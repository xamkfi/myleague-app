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
/// Query for retrieving top scorers for a specific season
/// </summary>
/// <param name="SeasonId">The season ID</param>
/// <param name="TopN">Number of top scorers to return (default: 10)</param>
public record GetTopScorersQuery(Guid CompetitionId, int TopN = 10) : IRequest<Result<List<FloorballPlayerSeasonStatisticsDto>>>;
