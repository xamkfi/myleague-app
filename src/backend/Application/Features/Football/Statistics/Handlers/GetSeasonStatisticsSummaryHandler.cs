using Application.Common;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Mappings;
using Application.Features.Football.Statistics.Queries;
using Application.Features.Football.Teams.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Common;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Statistics.Handlers;

/// <summary>
/// Handler for retrieving season statistics summary
/// </summary>
public class GetSeasonStatisticsSummaryHandler : IRequestHandler<GetSeasonStatisticsSummaryQuery, Result<FootballSeasonStatisticsSummaryDto>>
{
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballPlayerRepository _footballPlayerRepository;
    private readonly IFootballMatchRepository _footballMatchRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetSeasonStatisticsSummaryHandler> _logger;

    public GetSeasonStatisticsSummaryHandler(
        IFootballStatisticsRepository statisticsRepository,
        IFootballPlayerRepository footballPlayerRepository,
        IFootballMatchRepository footballMatchRepository,
        IPersonRepository personRepository,
        ILogger<GetSeasonStatisticsSummaryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _footballPlayerRepository = footballPlayerRepository;
        _footballMatchRepository = footballMatchRepository;
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<Result<FootballSeasonStatisticsSummaryDto>> Handle(GetSeasonStatisticsSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting season statistics summary for Season: {SeasonId}", request.CompetitionId);

            List<FootballTeamSeasonStatistics> teamStats =
                (await _statisticsRepository.GetTeamStandingsAsync(request.CompetitionId, cancellationToken)).ToList();

            List<FootballPlayerSeasonStatistics> topScorers =
                (await _statisticsRepository.GetTopScorersAsync(request.CompetitionId, 10, cancellationToken)).ToList();

            List<FootballPlayerSeasonStatistics> topAssists =
                (await _statisticsRepository.GetTopAssistsAsync(request.CompetitionId, 10, cancellationToken)).ToList();

            if (teamStats.Count == 0)
            {
                _logger.LogWarning("Season statistics not found for Season: {SeasonId}", request.CompetitionId);
                return Result<FootballSeasonStatisticsSummaryDto>.NotFound("Season statistics", request.CompetitionId.ToString());
            }

            bool isTournament = teamStats[0].Competition is FootballTournament;
            FootballStandingRules standingRules = teamStats[0].Competition?.StandingRules ?? FootballStandingRules.Default();
            List<FootballMatch> tournamentMatches = new List<FootballMatch>();
            Dictionary<Guid, TournamentTeamAggregate>? tournamentAggregates = null;
            if (isTournament)
            {
                tournamentMatches = (await _footballMatchRepository.GetByCompetitionIdAsync(request.CompetitionId)).ToList();
                tournamentAggregates = BuildTournamentGroupStageAggregates(tournamentMatches, standingRules);
            }

            Dictionary<Guid, FootballGameResult[]> last5ByTeam = new Dictionary<Guid, FootballGameResult[]>();
            foreach (FootballTeamSeasonStatistics ts in teamStats)
            {
                IEnumerable<FootballMatch> matches;
                if (isTournament)
                {
                    matches = tournamentMatches
                        .Where(m => m.TournamentGroupId != null
                            && m.Status == FootballMatchStatus.Completed
                            && (m.HomeTeamId == ts.TeamId || m.AwayTeamId == ts.TeamId))
                        .OrderByDescending(m => m.ScheduledDateTime)
                        .Take(5);
                }
                else
                {
                    matches = await _footballMatchRepository.GetLastCompletedByTeamAsync(ts.TeamId, request.CompetitionId, 5);
                }

                FootballGameResult[] form = matches.Select(m =>
                {
                    if (m.HomeScore == m.AwayScore) return FootballGameResult.Draw;
                    bool teamIsHome = m.HomeTeamId == ts.TeamId;
                    bool teamWon = (teamIsHome && m.HomeScore > m.AwayScore) || (!teamIsHome && m.AwayScore > m.HomeScore);
                    return teamWon ? FootballGameResult.Win : FootballGameResult.Loss;
                }).ToArray();

                last5ByTeam[ts.TeamId] = form;
            }

            string seasonName = teamStats.FirstOrDefault()?.Competition?.Name ?? string.Empty;

            Dictionary<Guid, Person> playerPersonLookup = new Dictionary<Guid, Person>();

            IEnumerable<Guid> playerIds = topScorers.Select(ps => ps.PlayerId)
                .Concat(topAssists.Select(ps => ps.PlayerId))
                .Distinct()
                .ToList();

            if (playerIds.Any())
            {
                List<FootballPlayer> players = new List<FootballPlayer>();
                foreach (Guid playerId in playerIds)
                {
                    FootballPlayer? player = await _footballPlayerRepository.GetByIdAsync(playerId);
                    if (player != null)
                    {
                        players.Add(player);
                    }
                }

                List<Guid> personIds = players.Select(p => p.PersonId).Distinct().ToList();

                if (personIds.Any())
                {
                    IEnumerable<Person> persons = await _personRepository.GetByIdsAsync(personIds);
                    Dictionary<Guid, Person> personLookup = persons.ToDictionary(p => p.Id, p => p);

                    foreach (FootballPlayer player in players)
                    {
                        if (personLookup.TryGetValue(player.PersonId, out Person? person))
                        {
                            playerPersonLookup[player.Id] = person;
                        }
                    }
                }
            }

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

            FootballSeasonStatisticsSummaryDto summaryDto = new FootballSeasonStatisticsSummaryDto
            {
                CompetitionId = request.CompetitionId,
                SeasonName = seasonName,
                TeamStandings = teamStats.Select(ts => FootballStatisticsMapper.ToDto(ts)).ToList(),
                TopScorers = topScorers.Select(ps =>
                {
                    string playerName = playerPersonLookup.TryGetValue(ps.PlayerId, out Person? person)
                        ? person.FullName
                        : string.Empty;
                    return FootballStatisticsMapper.ToDto(ps, playerName);
                }).ToList(),
                TopAssists = topAssists.Select(ps =>
                {
                    string playerName = playerPersonLookup.TryGetValue(ps.PlayerId, out Person? person)
                        ? person.FullName
                        : string.Empty;
                    return FootballStatisticsMapper.ToDto(ps, playerName);
                }).ToList(),
                TotalGames = totalGames,
                TotalGoals = totalGoals,
                AverageGoalsPerGame = averageGoalsPerGame
            };

            if (isTournament && tournamentAggregates != null)
            {
                foreach (FootballTeamSeasonStatisticsDto teamDto in summaryDto.TeamStandings)
                {
                    if (tournamentAggregates.TryGetValue(teamDto.TeamId, out TournamentTeamAggregate? agg))
                    {
                        teamDto.GamesPlayed = agg.GamesPlayed;
                        teamDto.Wins = agg.Wins;
                        teamDto.Losses = agg.Losses;
                        teamDto.Draws = agg.Draws;
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
                        teamDto.Draws = 0;
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

            foreach (FootballTeamSeasonStatisticsDto teamDto in summaryDto.TeamStandings)
            {
                if (last5ByTeam.TryGetValue(teamDto.TeamId, out FootballGameResult[]? form))
                {
                    teamDto.LastFiveForm = form;
                }
            }

            _logger.LogInformation("Successfully retrieved season statistics summary for Season: {SeasonId}", request.CompetitionId);
            return Result<FootballSeasonStatisticsSummaryDto>.Success(summaryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting season statistics summary for Season: {SeasonId}", request.CompetitionId);
            return Result<FootballSeasonStatisticsSummaryDto>.Failure("An error occurred while retrieving season statistics summary.");
        }
    }

    private static Dictionary<Guid, TournamentTeamAggregate> BuildTournamentGroupStageAggregates(
        IEnumerable<FootballMatch> tournamentMatches,
        FootballStandingRules standingRules)
    {
        Dictionary<Guid, TournamentTeamAggregate> rows = new Dictionary<Guid, TournamentTeamAggregate>();

        foreach (FootballMatch m in tournamentMatches)
        {
            if (m.TournamentGroupId == null || m.Status != FootballMatchStatus.Completed)
                continue;

            if (!m.HomeTeamId.HasValue || !m.AwayTeamId.HasValue)
                continue;

            Guid homeId = m.HomeTeamId.Value;
            Guid awayId = m.AwayTeamId.Value;

            TournamentTeamAggregate home = rows.TryGetValue(homeId, out TournamentTeamAggregate? h)
                ? h
                : rows[homeId] = new TournamentTeamAggregate(standingRules);
            TournamentTeamAggregate away = rows.TryGetValue(awayId, out TournamentTeamAggregate? a)
                ? a
                : rows[awayId] = new TournamentTeamAggregate(standingRules);

            home.Apply(m.HomeScore, m.AwayScore);
            away.Apply(m.AwayScore, m.HomeScore);
        }

        return rows;
    }

    private sealed class TournamentTeamAggregate
    {
        private readonly FootballStandingRules _standingRules;

        public TournamentTeamAggregate(FootballStandingRules standingRules)
        {
            _standingRules = standingRules;
        }

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
                Points += _standingRules.WinPoints;
            }
            else if (scoredFor < scoredAgainst)
            {
                Losses++;
                Points += _standingRules.LossPoints;
            }
            else
            {
                Draws++;
                Points += _standingRules.DrawPoints;
            }
        }
    }
}
