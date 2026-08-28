using Application.Common;
using Application.Constants;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Records a batch of historical goals and penalties on a started floorball match
/// in one unit of work, skipping the live per-event rate limiter.
/// </summary>
public class ImportFloorballMatchEventsHandler
    : IRequestHandler<ImportFloorballMatchEventsCommand, Result<FloorballMatchEventsImportDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<ImportFloorballMatchEventsHandler> _logger;

    public ImportFloorballMatchEventsHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<ImportFloorballMatchEventsHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchEventsImportDto>> Handle(
        ImportFloorballMatchEventsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FloorballMatchEventsImportDto>.NotFound("FloorballMatch", request.MatchId);
            }

            if (match.Status != FloorballMatchStatus.InProgress)
            {
                return Result<FloorballMatchEventsImportDto>.Failure(
                    $"Match must be in progress to import events. Current status: {match.Status}");
            }

            Dictionary<Guid, FloorballTeam> teams = new();
            Dictionary<Guid, FloorballPlayer> players = new();
            StatsBuffer stats = new(_statisticsRepository);

            int goalsRecorded = 0;
            int penaltiesRecorded = 0;
            List<string> eventErrors = new();

            for (int index = 0; index < request.Events.Count; index++)
            {
                ImportFloorballMatchEventItem item = request.Events[index];
                try
                {
                    if (string.Equals(item.EventType, "Goal", StringComparison.OrdinalIgnoreCase))
                    {
                        await RecordImportedGoalAsync(match, item, teams, players, stats, cancellationToken);
                        goalsRecorded++;
                    }
                    else if (string.Equals(item.EventType, "Penalty", StringComparison.OrdinalIgnoreCase))
                    {
                        await RecordImportedPenaltyAsync(match, item, teams, players, stats, cancellationToken);
                        penaltiesRecorded++;
                    }
                    else
                    {
                        eventErrors.Add($"[{index}] Unknown event type '{item.EventType}'.");
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
                {
                    eventErrors.Add($"[{index}] {item.EventType}: {ex.Message}");
                    _logger.LogWarning(
                        ex,
                        "Skipped import event {Index} of type {EventType} on match {MatchId}",
                        index,
                        item.EventType,
                        request.MatchId);
                }
            }

            await stats.FlushAsync(cancellationToken);
            await _matchRepository.UpdateAsync(match);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (goalsRecorded > 0)
            {
                await _notificationSenderService.SendNotificationAsync(
                    FloorballNotificationEvents.GoalScored,
                    new MatchNotificationPayload(match.Id));
            }

            if (penaltiesRecorded > 0)
            {
                await _notificationSenderService.SendNotificationAsync(
                    FloorballNotificationEvents.PenaltyAssigned,
                    new MatchNotificationPayload(match.Id));
            }

            FloorballMatchEventsImportDto dto = new(
                FloorballMatchMapper.ToDto(match),
                goalsRecorded,
                penaltiesRecorded,
                eventErrors);

            _logger.LogInformation(
                "Imported {GoalCount} goals and {PenaltyCount} penalties on match {MatchId} ({ErrorCount} event errors)",
                goalsRecorded,
                penaltiesRecorded,
                request.MatchId,
                eventErrors.Count);

            return Result<FloorballMatchEventsImportDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed importing events for match {MatchId}", request.MatchId);
            string detail = ex.InnerException?.Message ?? ex.Message;
            return Result<FloorballMatchEventsImportDto>.Failure(detail, ex.Flatten());
        }
    }

    private async Task RecordImportedGoalAsync(
        FloorballMatch match,
        ImportFloorballMatchEventItem item,
        Dictionary<Guid, FloorballTeam> teams,
        Dictionary<Guid, FloorballPlayer> players,
        StatsBuffer stats,
        CancellationToken cancellationToken)
    {
        if (!item.PlayerId.HasValue)
        {
            throw new ArgumentException("Scoring player is required for a goal.");
        }

        FloorballTeam scoringTeam = await GetTeamAsync(item.TeamId, teams);
        FloorballPlayer scoringPlayer = await GetPlayerAsync(item.PlayerId.Value, players);
        FloorballPlayer? assister = item.AssistingPlayerId.HasValue
            ? await GetPlayerAsync(item.AssistingPlayerId.Value, players)
            : null;
        FloorballPlayer? secondAssister = item.SecondaryAssistingPlayerId.HasValue
            ? await GetPlayerAsync(item.SecondaryAssistingPlayerId.Value, players)
            : null;

        FloorballGoal goal = match.RecordGoal(
            scoringTeam,
            scoringPlayer,
            assister,
            secondAssister,
            item.PeriodNumber,
            item.TimeInSeconds,
            item.Description,
            item.GoalType);

        _matchRepository.MarkEventAsAdded(goal);

        bool isOwnGoal = goal.GoalType == FloorballGoalType.OwnGoal
            || item.GoalType == (int)FloorballGoalType.OwnGoal;

        if (!isOwnGoal)
        {
            scoringPlayer.RecordGoal();
            await stats.ApplyPlayerAsync(
                scoringPlayer.Id, item.TeamId, match.CompetitionId, isGoal: true, isAssist: false, cancellationToken);
            if (assister != null)
            {
                assister.RecordAssist();
                await stats.ApplyPlayerAsync(
                    assister.Id, item.TeamId, match.CompetitionId, isGoal: false, isAssist: true, cancellationToken);
            }

            if (secondAssister != null)
            {
                secondAssister.RecordAssist();
                await stats.ApplyPlayerAsync(
                    secondAssister.Id, item.TeamId, match.CompetitionId, isGoal: false, isAssist: true, cancellationToken);
            }
        }

        await stats.ApplyTeamGoalAsync(item.TeamId, match.CompetitionId, isGoalFor: true, cancellationToken);
        Guid opposingTeamId = (item.TeamId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId)!.Value;
        await stats.ApplyTeamGoalAsync(opposingTeamId, match.CompetitionId, isGoalFor: false, cancellationToken);
        await stats.ApplyMatchShotsAsync(match.Id, item.TeamId, cancellationToken);

        if (!isOwnGoal)
        {
            Guid? activeGoalieId = match.GetActiveGoalieId(opposingTeamId);
            if (activeGoalieId.HasValue)
            {
                await stats.ApplyGoalieGoalAgainstAsync(
                    activeGoalieId.Value, opposingTeamId, match.CompetitionId, cancellationToken);
            }
        }
    }

    private async Task RecordImportedPenaltyAsync(
        FloorballMatch match,
        ImportFloorballMatchEventItem item,
        Dictionary<Guid, FloorballTeam> teams,
        Dictionary<Guid, FloorballPlayer> players,
        StatsBuffer stats,
        CancellationToken cancellationToken)
    {
        if (!item.PlayerId.HasValue)
        {
            throw new ArgumentException("Player is required for a penalty.");
        }

        FloorballTeam team = await GetTeamAsync(item.TeamId, teams);
        FloorballPlayer player = await GetPlayerAsync(item.PlayerId.Value, players);

        if (!Enum.TryParse(item.PenaltyType, ignoreCase: true, out FloorballPenaltyType penaltyType)
            || penaltyType == FloorballPenaltyType.None)
        {
            penaltyType = FloorballPenaltyType.Minor;
        }

        int minutes = item.PenaltyMinutes ?? 2;
        FloorballPenalty penalty = match.RecordPenalty(
            team,
            player,
            penaltyType,
            minutes,
            item.PeriodNumber,
            item.TimeInSeconds,
            item.Description ?? string.Empty);

        _matchRepository.MarkEventAsAdded(penalty);
        await stats.ApplyPlayerPenaltyAsync(player.Id, item.TeamId, match.CompetitionId, minutes, cancellationToken);
        await stats.ApplyMatchPenaltyAsync(match.Id, item.TeamId, minutes, cancellationToken);
    }

    private async Task<FloorballTeam> GetTeamAsync(Guid teamId, Dictionary<Guid, FloorballTeam> cache)
    {
        if (cache.TryGetValue(teamId, out FloorballTeam? cached))
        {
            return cached;
        }

        FloorballTeam? team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            throw new InvalidOperationException($"Team with ID {teamId} not found.");
        }

        cache[teamId] = team;
        return team;
    }

    private async Task<FloorballPlayer> GetPlayerAsync(Guid playerId, Dictionary<Guid, FloorballPlayer> cache)
    {
        if (cache.TryGetValue(playerId, out FloorballPlayer? cached))
        {
            return cached;
        }

        FloorballPlayer? player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new InvalidOperationException($"Player with ID {playerId} not found.");
        }

        cache[playerId] = player;
        return player;
    }

    private sealed class StatsBuffer
    {
        private readonly IFloorballStatisticsRepository _repository;
        private readonly Dictionary<(Guid PlayerId, Guid TeamId, Guid SeasonId), FloorballPlayerSeasonStatistics> _players = new();
        private readonly Dictionary<(Guid TeamId, Guid SeasonId), FloorballTeamSeasonStatistics> _teams = new();
        private readonly Dictionary<(Guid MatchId, Guid TeamId), FloorballMatchTeamStatistics> _matchTeams = new();
        private readonly Dictionary<(Guid PlayerId, Guid TeamId, Guid SeasonId), FloorballGoalieSeasonStatistics> _goalies = new();

        public StatsBuffer(IFloorballStatisticsRepository repository)
        {
            _repository = repository;
        }

        public async Task ApplyPlayerAsync(
            Guid playerId,
            Guid teamId,
            Guid seasonId,
            bool isGoal,
            bool isAssist,
            CancellationToken cancellationToken)
        {
            FloorballPlayerSeasonStatistics stats = await GetOrCreatePlayerAsync(
                playerId, teamId, seasonId, cancellationToken);
            if (isGoal)
            {
                stats.RecordGoal();
            }

            if (isAssist)
            {
                stats.RecordAssist();
            }
        }

        public async Task ApplyPlayerPenaltyAsync(
            Guid playerId,
            Guid teamId,
            Guid seasonId,
            int minutes,
            CancellationToken cancellationToken)
        {
            FloorballPlayerSeasonStatistics stats = await GetOrCreatePlayerAsync(
                playerId, teamId, seasonId, cancellationToken);
            stats.RecordPenaltyMinutes(minutes);
        }

        public async Task ApplyTeamGoalAsync(
            Guid teamId,
            Guid seasonId,
            bool isGoalFor,
            CancellationToken cancellationToken)
        {
            (Guid TeamId, Guid SeasonId) key = (teamId, seasonId);
            if (!_teams.TryGetValue(key, out FloorballTeamSeasonStatistics? stats))
            {
                FloorballTeamSeasonStatistics? existing =
                    await _repository.GetTeamSeasonStatisticsAsync(teamId, seasonId, cancellationToken);
                stats = existing ?? new FloorballTeamSeasonStatistics(teamId, seasonId);
                _teams[key] = stats;
            }

            if (isGoalFor)
            {
                stats.IncrementGoalsFor();
            }
            else
            {
                stats.IncrementGoalsAgainst();
            }
        }

        public async Task ApplyMatchShotsAsync(Guid matchId, Guid teamId, CancellationToken cancellationToken)
        {
            FloorballMatchTeamStatistics stats = await GetOrCreateMatchTeamAsync(matchId, teamId, cancellationToken);
            stats.UpdateShotStatistics(1, 1);
        }

        public async Task ApplyMatchPenaltyAsync(
            Guid matchId,
            Guid teamId,
            int minutes,
            CancellationToken cancellationToken)
        {
            FloorballMatchTeamStatistics stats = await GetOrCreateMatchTeamAsync(matchId, teamId, cancellationToken);
            stats.UpdatePenaltyMinutes(minutes);
        }

        public async Task ApplyGoalieGoalAgainstAsync(
            Guid goalieId,
            Guid teamId,
            Guid seasonId,
            CancellationToken cancellationToken)
        {
            (Guid PlayerId, Guid TeamId, Guid SeasonId) key = (goalieId, teamId, seasonId);
            if (!_goalies.TryGetValue(key, out FloorballGoalieSeasonStatistics? stats))
            {
                FloorballGoalieSeasonStatistics? existing =
                    await _repository.GetGoalieSeasonStatisticsAsync(goalieId, teamId, seasonId, cancellationToken);
                stats = existing ?? new FloorballGoalieSeasonStatistics(goalieId, teamId, seasonId);
                _goalies[key] = stats;
            }

            stats.RecordSaves(0, 1, 1);
        }

        public async Task FlushAsync(CancellationToken cancellationToken)
        {
            foreach (FloorballPlayerSeasonStatistics stats in _players.Values)
            {
                await _repository.SavePlayerSeasonStatisticsAsync(stats, cancellationToken);
            }

            foreach (FloorballTeamSeasonStatistics stats in _teams.Values)
            {
                await _repository.SaveTeamSeasonStatisticsAsync(stats, cancellationToken);
            }

            foreach (FloorballMatchTeamStatistics stats in _matchTeams.Values)
            {
                await _repository.SaveMatchTeamStatisticsAsync(stats, cancellationToken);
            }

            foreach (FloorballGoalieSeasonStatistics stats in _goalies.Values)
            {
                await _repository.SaveGoalieSeasonStatisticsAsync(stats, cancellationToken);
            }
        }

        private async Task<FloorballPlayerSeasonStatistics> GetOrCreatePlayerAsync(
            Guid playerId,
            Guid teamId,
            Guid seasonId,
            CancellationToken cancellationToken)
        {
            (Guid PlayerId, Guid TeamId, Guid SeasonId) key = (playerId, teamId, seasonId);
            if (_players.TryGetValue(key, out FloorballPlayerSeasonStatistics? cached))
            {
                return cached;
            }

            FloorballPlayerSeasonStatistics? existing =
                await _repository.GetPlayerSeasonStatisticsAsync(playerId, teamId, seasonId, cancellationToken);
            FloorballPlayerSeasonStatistics stats = existing ?? new FloorballPlayerSeasonStatistics(playerId, teamId, seasonId);
            _players[key] = stats;
            return stats;
        }

        private async Task<FloorballMatchTeamStatistics> GetOrCreateMatchTeamAsync(
            Guid matchId,
            Guid teamId,
            CancellationToken cancellationToken)
        {
            (Guid MatchId, Guid TeamId) key = (matchId, teamId);
            if (_matchTeams.TryGetValue(key, out FloorballMatchTeamStatistics? cached))
            {
                return cached;
            }

            FloorballMatchTeamStatistics? existing =
                await _repository.GetMatchTeamStatisticsAsync(matchId, teamId, cancellationToken);
            FloorballMatchTeamStatistics stats = existing ?? new FloorballMatchTeamStatistics(matchId, teamId);
            _matchTeams[key] = stats;
            return stats;
        }
    }
}
