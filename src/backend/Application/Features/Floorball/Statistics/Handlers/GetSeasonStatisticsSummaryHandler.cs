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

            // For tournaments the team-standings table (W/L/T/Pts) must only reflect group-stage
            // matches — playoff results should not pollute the league-style table. Top scorers/
            // assists/goalies still cover the whole tournament span (those are individual awards).
            bool isTournament = teamStats[0].Competition is Domain.Entities.Floorball.FloorballTournament;
            List<Domain.Entities.Floorball.FloorballMatch> tournamentMatches = new List<Domain.Entities.Floorball.FloorballMatch>();
            Dictionary<Guid, TournamentTeamAggregate>? tournamentAggregates = null;
            if (isTournament)
            {
                tournamentMatches = (await _floorballMatchRepository.GetByCompetitionIdAsync(request.CompetitionId)).ToList();
                tournamentAggregates = BuildTournamentGroupStageAggregates(tournamentMatches);
            }

            // Build last-5 form per team. For tournaments we restrict to group-stage completed matches
            // so the badges on the standings table match the standings values themselves.
            Dictionary<Guid, FloorballGameResult[]> last5ByTeam = new Dictionary<Guid, FloorballGameResult[]>();
            foreach (Domain.Entities.Floorball.FloorballTeamSeasonStatistics ts in teamStats)
            {
                IEnumerable<Domain.Entities.Floorball.FloorballMatch> matches;
                if (isTournament)
                {
                    matches = tournamentMatches
                        .Where(m => m.TournamentGroupId != null
                            && m.Status == FloorballMatchStatus.Completed
                            && (m.HomeTeamId == ts.TeamId || m.AwayTeamId == ts.TeamId))
                        .OrderByDescending(m => m.ScheduledDateTime)
                        .Take(5);
                }
                else
                {
                    matches = await _floorballMatchRepository.GetLastCompletedByTeamAsync(ts.TeamId, request.CompetitionId, 5);
                }

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

            // Calculate summary statistics. For tournaments use the group-stage-only aggregates so
            // the totals match the visible standings table; for seasons keep the existing
            // statistics-store based totals.
            int totalGames;
            int totalGoals;
            if (isTournament && tournamentAggregates != null)
            {
                totalGames = tournamentAggregates.Values.Sum(a => a.GamesPlayed) / 2;
                totalGoals = tournamentAggregates.Values.Sum(a => a.GoalsFor);
            }
            else
            {
                totalGames = teamStats.Sum(ts => ts.GamesPlayed) / 2;
                totalGoals = teamStats.Sum(ts => ts.GoalsFor);
            }
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
            
            // For tournaments, override the standings W/L/T/Pts/GoalsFor/GoalsAgainst with group-stage
            // only aggregates so the public standings table doesn't double-count playoff results. We
            // re-sort by the new points so the table order matches the displayed values.
            if (isTournament && tournamentAggregates != null)
            {
                foreach (FloorballTeamSeasonStatisticsDto teamDto in summaryDto.TeamStandings)
                {
                    if (tournamentAggregates.TryGetValue(teamDto.TeamId, out TournamentTeamAggregate? agg))
                    {
                        teamDto.GamesPlayed = agg.GamesPlayed;
                        teamDto.Wins = agg.Wins;
                        teamDto.Losses = agg.Losses;
                        teamDto.Ties = agg.Draws;
                        teamDto.Points = agg.Points;
                        teamDto.GoalsFor = agg.GoalsFor;
                        teamDto.GoalsAgainst = agg.GoalsAgainst;
                        teamDto.GoalDifference = agg.GoalsFor - agg.GoalsAgainst;
                    }
                    else
                    {
                        teamDto.GamesPlayed = 0;
                        teamDto.Wins = 0;
                        teamDto.Losses = 0;
                        teamDto.Ties = 0;
                        teamDto.Points = 0;
                        teamDto.GoalsFor = 0;
                        teamDto.GoalsAgainst = 0;
                        teamDto.GoalDifference = 0;
                    }
                }

                summaryDto.TeamStandings = summaryDto.TeamStandings
                    .OrderByDescending(t => t.Points)
                    .ThenByDescending(t => t.GoalDifference)
                    .ThenByDescending(t => t.GoalsFor)
                    .ThenBy(t => t.TeamName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

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

    /// <summary>
    /// Aggregates per-team group-stage results across all groups in a tournament. We don't reuse
    /// TournamentStandingsCalculator directly because that calculator works per-group; here we need
    /// one row per team summing across all of its group-stage games.
    /// </summary>
    private static Dictionary<Guid, TournamentTeamAggregate> BuildTournamentGroupStageAggregates(
        IEnumerable<Domain.Entities.Floorball.FloorballMatch> tournamentMatches)
    {
        Dictionary<Guid, TournamentTeamAggregate> rows = new Dictionary<Guid, TournamentTeamAggregate>();

        foreach (Domain.Entities.Floorball.FloorballMatch m in tournamentMatches)
        {
            if (m.TournamentGroupId == null || m.Status != FloorballMatchStatus.Completed)
                continue;

            // Completed matches always have both team IDs assigned; defensively skip anything that
            // doesn't so the aggregate query can never NRE on malformed historical data.
            if (!m.HomeTeamId.HasValue || !m.AwayTeamId.HasValue)
                continue;

            Guid homeId = m.HomeTeamId.Value;
            Guid awayId = m.AwayTeamId.Value;

            TournamentTeamAggregate home = rows.TryGetValue(homeId, out TournamentTeamAggregate? h)
                ? h
                : rows[homeId] = new TournamentTeamAggregate();
            TournamentTeamAggregate away = rows.TryGetValue(awayId, out TournamentTeamAggregate? a)
                ? a
                : rows[awayId] = new TournamentTeamAggregate();

            home.Apply(m.HomeScore, m.AwayScore);
            away.Apply(m.AwayScore, m.HomeScore);
        }

        return rows;
    }

    private sealed class TournamentTeamAggregate
    {
        public int GamesPlayed { get; private set; }
        public int Wins { get; private set; }
        public int Draws { get; private set; }
        public int Losses { get; private set; }
        public int GoalsFor { get; private set; }
        public int GoalsAgainst { get; private set; }
        public int Points { get; private set; }

        public void Apply(int scoredFor, int scoredAgainst)
        {
            GamesPlayed++;
            GoalsFor += scoredFor;
            GoalsAgainst += scoredAgainst;
            if (scoredFor > scoredAgainst)
            {
                Wins++;
                Points += 3;
            }
            else if (scoredFor < scoredAgainst)
            {
                Losses++;
            }
            else
            {
                Draws++;
                Points += 1;
            }
        }
    }
}
