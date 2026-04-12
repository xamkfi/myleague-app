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
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Domain.Entities.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Statistics.Handlers;

/// <summary>
/// Handler for retrieving season statistics summary
/// </summary>
public class GetSeasonStatisticsSummaryHandler : IRequestHandler<GetSeasonStatisticsSummaryQuery, Result<FloorballSeasonStatisticsSummaryDto>>
{
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballPlayerRepository _floorballPlayerRepository;
    private readonly IFloorballTeamRepository _floorballTeamRepository;
    private readonly IFloorballMatchRepository _floorballMatchRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetSeasonStatisticsSummaryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetSeasonStatisticsSummaryHandler class
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="logger">The logger</param>
    public GetSeasonStatisticsSummaryHandler(
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballPlayerRepository floorballPlayerRepository,
        IFloorballTeamRepository floorballTeamRepository,
        IFloorballMatchRepository floorballMatchRepository,
        IPersonRepository personRepository,
        ILogger<GetSeasonStatisticsSummaryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _floorballPlayerRepository = floorballPlayerRepository;
        _floorballTeamRepository = floorballTeamRepository;
        _floorballMatchRepository = floorballMatchRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetSeasonStatisticsSummaryQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing season statistics summary DTO</returns>
    public async Task<Result<FloorballSeasonStatisticsSummaryDto>> Handle(GetSeasonStatisticsSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting season statistics summary for Season: {SeasonId}", request.CompetitionId);

            // Get team standings
            List<Domain.Entities.Floorball.FloorballTeamSeasonStatistics> teamStats = 
                (await _statisticsRepository.GetTeamStandingsAsync(request.CompetitionId, cancellationToken)).ToList();

            // Get top scorers
            List<Domain.Entities.Floorball.FloorballPlayerSeasonStatistics> topScorers = 
                (await _statisticsRepository.GetTopScorersAsync(request.CompetitionId, 10, cancellationToken)).ToList();

            // Get top assist leaders
            List<Domain.Entities.Floorball.FloorballPlayerSeasonStatistics> topAssists = 
                (await _statisticsRepository.GetTopAssistsAsync(request.CompetitionId, 10, cancellationToken)).ToList();

            // Get top goalies (minimum 5 games played)
            List<Domain.Entities.Floorball.FloorballGoalieSeasonStatistics> topGoalies = 
                (await _statisticsRepository.GetTopGoaliesAsync(request.CompetitionId, 10, 1, cancellationToken)).ToList();

            if (teamStats.Count == 0)
            {
                _logger.LogWarning("Season statistics not found for Season: {SeasonId}", request.CompetitionId);
                return Result<FloorballSeasonStatisticsSummaryDto>.NotFound("Season statistics", request.CompetitionId.ToString());
            }

            // Build last-5 form per team
            Dictionary<Guid, FloorballGameResult[]> last5ByTeam = new Dictionary<Guid, FloorballGameResult[]>();
            foreach (Domain.Entities.Floorball.FloorballTeamSeasonStatistics ts in teamStats)
            {
                IEnumerable<Domain.Entities.Floorball.FloorballMatch> matches =
                    await _floorballMatchRepository.GetLastCompletedByTeamAsync(ts.TeamId, request.CompetitionId, 5);

                FloorballGameResult[] form = matches.Select(m =>
                {
                    if (m.HomeScore == m.AwayScore) return FloorballGameResult.Tie;
                    bool teamIsHome = m.HomeTeamId == ts.TeamId;
                    bool teamWon = (teamIsHome && m.HomeScore > m.AwayScore) || (!teamIsHome && m.AwayScore > m.HomeScore);
                    return teamWon ? FloorballGameResult.Win : FloorballGameResult.Loss;
                }).ToArray();

                last5ByTeam[ts.TeamId] = form;
            }

            // Create lookups for team names and season names from team standings
            Dictionary<Guid, string> teamNameLookup = teamStats.ToDictionary(ts => ts.TeamId, ts => ts.Team.Name ?? string.Empty);
            string seasonName = teamStats.FirstOrDefault()?.Competition.Name ?? string.Empty;

            // Retrieve player names from person repository for top scorers and assists
            Dictionary<Guid, Person> playerPersonLookup = new Dictionary<Guid, Person>();
            
            // Get all unique player IDs from top scorers, assists, and goalies
            IEnumerable<Guid> playerIds = topScorers.Select(ps => ps.PlayerId)
                .Concat(topAssists.Select(ps => ps.PlayerId))
                .Concat(topGoalies.Select(gs => gs.PlayerId))
                .Distinct()
                .ToList();

            if (playerIds.Any())
            {
                // Get players to map player ID to person ID
                List<Domain.Entities.Floorball.FloorballPlayer> players = new List<Domain.Entities.Floorball.FloorballPlayer>();
                foreach (Guid playerId in playerIds)
                {
                    Domain.Entities.Floorball.FloorballPlayer? player = await _floorballPlayerRepository.GetByIdAsync(playerId);
                    if (player != null)
                    {
                        players.Add(player);
                    }
                }

                // Extract person IDs from players
                List<Guid> personIds = players.Select(p => p.PersonId).Distinct().ToList();
                
                // Load persons using PersonRepository
                if (personIds.Any())
                {
                    IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);
                    Dictionary<Guid, Person> personLookup = persons.ToDictionary(p => p.Id, p => p);
                    
                    // Create lookup from player ID to person
                    foreach (Domain.Entities.Floorball.FloorballPlayer player in players)
                    {
                        if (personLookup.TryGetValue(player.PersonId, out Person? person))
                        {
                            playerPersonLookup[player.Id] = person;
                        }
                    }
                }
            }

            // Calculate summary statistics
            int totalGames = teamStats.Sum(ts => ts.GamesPlayed) / 2; // Divide by 2 since each game involves 2 teams
            int totalGoals = teamStats.Sum(ts => ts.GoalsFor);
            decimal averageGoalsPerGame = totalGames > 0 ? (decimal)totalGoals / totalGames : 0;

            FloorballSeasonStatisticsSummaryDto summaryDto = new FloorballSeasonStatisticsSummaryDto
            {
                CompetitionId = request.CompetitionId,
                SeasonName = seasonName,
                TeamStandings = teamStats.Select(ts => FloorballStatisticsMapper.ToDto(ts)).ToList(),
                TopScorers = topScorers.Select(ps =>
                {
                    string playerName = playerPersonLookup.TryGetValue(ps.PlayerId, out Person? person)
                        ? person.FullName
                        : string.Empty;
                    string teamName = teamNameLookup.TryGetValue(ps.TeamId, out string? team) ? team : string.Empty;
                    return FloorballStatisticsMapper.ToDto(ps, playerName);
                }).ToList(),
                TopAssists = topAssists.Select(ps =>
                {
                    string playerName = playerPersonLookup.TryGetValue(ps.PlayerId, out Person? person)
                        ? person.FullName
                        : string.Empty;
                    return FloorballStatisticsMapper.ToDto(ps, playerName);
                }).ToList(),
                TopGoalies = topGoalies.Select(gs =>
                {
                    string playerName = playerPersonLookup.TryGetValue(gs.PlayerId, out Person? person)
                        ? person.FullName
                        : string.Empty;
                    string teamName = teamNameLookup.TryGetValue(gs.TeamId, out string? team) ? team : string.Empty;
                    return FloorballStatisticsMapper.ToDto(gs, playerName);
                }).ToList(),
                TotalGames = totalGames,
                TotalGoals = totalGoals,
                AverageGoalsPerGame = averageGoalsPerGame
            };
            
            // Add last-5 form to each team's DTO
            foreach (FloorballTeamSeasonStatisticsDto teamDto in summaryDto.TeamStandings)
            {
                if (last5ByTeam.TryGetValue(teamDto.TeamId, out FloorballGameResult[]? form))
                {
                    teamDto.LastFiveForm = form;
                }
            }

            _logger.LogInformation("Successfully retrieved season statistics summary for Season: {SeasonId}", request.CompetitionId);
            return Result<FloorballSeasonStatisticsSummaryDto>.Success(summaryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting season statistics summary for Season: {SeasonId}", request.CompetitionId);
            return Result<FloorballSeasonStatisticsSummaryDto>.Failure("An error occurred while retrieving season statistics summary.");
        }
    }
}
