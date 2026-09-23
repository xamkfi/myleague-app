using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Application.Features.Floorball.Statistics.Queries;
using Domain.Entities.Floorball.Statistics;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Handler for retrieving team standings
/// </summary>
public class GetTeamStandingsHandler : IRequestHandler<GetFloorballTeamStandingsQuery, Result<List<FloorballTeamSeasonStatisticsDto>>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballCompetitionRepository _competitionRepository;
    private readonly IFloorballTournamentRepository _tournamentRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<GetTeamStandingsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetTeamStandingsHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="competitionRepository">The competition repository</param>
    /// <param name="tournamentRepository">The tournament repository</param>
    /// <param name="teamRepository">The team repository</param>
    /// <param name="logger">The logger</param>
    public GetTeamStandingsHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballCompetitionRepository competitionRepository,
        IFloorballTournamentRepository tournamentRepository,
        IFloorballTeamRepository teamRepository,
        ILogger<GetTeamStandingsHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _competitionRepository = competitionRepository;
        _tournamentRepository = tournamentRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballTeamStandingsQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of team season statistics DTOs ordered by standings</returns>
    public async Task<Result<List<FloorballTeamSeasonStatisticsDto>>> Handle(GetFloorballTeamStandingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting team standings for Season: {SeasonId}", request.CompetitionId);

            List<FloorballTeamSeasonStatistics> standings =
                await _statisticsRepository.GetTeamStandingsAsync(request.CompetitionId, cancellationToken);

            List<FloorballTeamSeasonStatisticsDto> standingsDtos = standings
                .Select(FloorballStatisticsMapper.ToDto)
                .ToList();

            standingsDtos = await FloorballEnrolledStandings.WithEnrolledZerosAsync(
                request.CompetitionId,
                standingsDtos,
                _competitionRepository,
                _tournamentRepository,
                _teamRepository,
                cancellationToken);

            _logger.LogInformation("Successfully retrieved team standings for Season: {SeasonId} - {Count} teams", request.CompetitionId, standingsDtos.Count);
            return Result<List<FloorballTeamSeasonStatisticsDto>>.Success(standingsDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting team standings for Season: {SeasonId}", request.CompetitionId);
            return Result<List<FloorballTeamSeasonStatisticsDto>>.Failure("An error occurred while retrieving team standings.");
        }
    }
}
