using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Statistics;

/// <summary>
/// Query for retrieving team standings for a specific season
/// </summary>
/// <param name="SeasonId">The season ID</param>
public record GetTeamStandingsQuery(Guid SeasonId) : IRequest<Result<List<FloorballTeamSeasonStatisticsDto>>>;
