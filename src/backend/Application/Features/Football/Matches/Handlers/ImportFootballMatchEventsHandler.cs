using Application.Common;
using Application.Constants;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Interfaces.Common;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Football.Matches.Handlers;

/// <summary>
/// Records a batch of historical goals and cards on a started football match
/// in one unit of work, skipping the live per-event rate limiter.
/// </summary>
public class ImportFootballMatchEventsHandler
    : IRequestHandler<ImportFootballMatchEventsCommand, Result<FootballMatchEventsImportDto>>
{
    private readonly IFootballMatchRepository _matchRepository;
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IFootballStatisticsRepository _statisticsRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly INotificationSenderService _notificationSenderService;
    private readonly ILogger<ImportFootballMatchEventsHandler> _logger;

    public ImportFootballMatchEventsHandler(
        IFootballMatchRepository matchRepository,
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IFootballStatisticsRepository statisticsRepository,
        IFootballUnitOfWork unitOfWork,
        INotificationSenderService notificationSenderService,
        ILogger<ImportFootballMatchEventsHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _notificationSenderService = notificationSenderService;
        _logger = logger;
    }

    public async Task<Result<FootballMatchEventsImportDto>> Handle(
        ImportFootballMatchEventsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            FootballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                return Result<FootballMatchEventsImportDto>.NotFound("FootballMatch", request.MatchId);
            }

            if (match.Status != FootballMatchStatus.InProgress)
            {
                return Result<FootballMatchEventsImportDto>.Failure(
                    $"Match must be in progress to import events. Current status: {match.Status}");
            }

            Dictionary<Guid, FootballTeam> teams = new();
            Dictionary<Guid, FootballPlayer> players = new();
            Dictionary<(Guid PlayerId, Guid TeamId, Guid SeasonId), FootballPlayerSeasonStatistics> playerStats = new();

            int goalsRecorded = 0;
            int cardsRecorded = 0;
            List<string> eventErrors = new();

            for (int index = 0; index < request.Events.Count; index++)
            {
                ImportFootballMatchEventItem item = request.Events[index];
                try
                {
                    if (string.Equals(item.EventType, "Goal", StringComparison.OrdinalIgnoreCase))
                    {
                        await RecordImportedGoalAsync(
                            match, item, teams, players, playerStats, cancellationToken);
                        goalsRecorded++;
                    }
                    else if (string.Equals(item.EventType, "Card", StringComparison.OrdinalIgnoreCase))
                    {
                        await RecordImportedCardAsync(
                            match, item, teams, players, playerStats, cancellationToken);
                        cardsRecorded++;
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

            foreach (FootballPlayerSeasonStatistics stats in playerStats.Values)
            {
                await _statisticsRepository.SavePlayerSeasonStatisticsAsync(stats, cancellationToken);
            }

            await _matchRepository.UpdateAsync(match);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (goalsRecorded > 0)
            {
                await _notificationSenderService.SendNotificationAsync(
                    FootballNotificationEvents.GoalScored,
                    new MatchNotificationPayload(match.Id));
            }

            if (cardsRecorded > 0)
            {
                await _notificationSenderService.SendNotificationAsync(
                    FootballNotificationEvents.CardAssigned,
                    new MatchNotificationPayload(match.Id));
            }

            FootballMatchEventsImportDto dto = new(
                FootballMatchMapper.ToDto(match),
                goalsRecorded,
                cardsRecorded,
                eventErrors);

            _logger.LogInformation(
                "Imported {GoalCount} goals and {CardCount} cards on match {MatchId} ({ErrorCount} event errors)",
                goalsRecorded,
                cardsRecorded,
                request.MatchId,
                eventErrors.Count);

            return Result<FootballMatchEventsImportDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed importing events for football match {MatchId}", request.MatchId);
            string detail = ex.InnerException?.Message ?? ex.Message;
            return Result<FootballMatchEventsImportDto>.Failure(detail, ex.Flatten());
        }
    }

    private async Task RecordImportedGoalAsync(
        FootballMatch match,
        ImportFootballMatchEventItem item,
        Dictionary<Guid, FootballTeam> teams,
        Dictionary<Guid, FootballPlayer> players,
        Dictionary<(Guid PlayerId, Guid TeamId, Guid SeasonId), FootballPlayerSeasonStatistics> playerStats,
        CancellationToken cancellationToken)
    {
        if (!item.PlayerId.HasValue)
        {
            throw new ArgumentException("Scoring player is required for a goal.");
        }

        FootballTeam scoringTeam = await GetTeamAsync(item.TeamId, teams);
        FootballPlayer scoringPlayer = await GetPlayerAsync(item.PlayerId.Value, players);
        FootballPlayer? assister = item.AssistingPlayerId.HasValue
            ? await GetPlayerAsync(item.AssistingPlayerId.Value, players)
            : null;

        FootballGoal goal = match.RecordGoal(
            scoringTeam,
            scoringPlayer,
            assister,
            item.PeriodNumber,
            item.TimeInSeconds,
            item.GoalType,
            item.Description);

        _matchRepository.MarkEventAsAdded(goal);

        bool isOwnGoal = goal.IsOwnGoal || item.GoalType == FootballGoalType.OwnGoal;
        if (!isOwnGoal)
        {
            scoringPlayer.RecordGoal();
            await MutatePlayerStatsAsync(
                playerStats, scoringPlayer.Id, item.TeamId, match.CompetitionId, isGoal: true, isAssist: false, cancellationToken);
            if (assister != null)
            {
                assister.RecordAssist();
                await MutatePlayerStatsAsync(
                    playerStats, assister.Id, item.TeamId, match.CompetitionId, isGoal: false, isAssist: true, cancellationToken);
            }
        }
    }

    private async Task RecordImportedCardAsync(
        FootballMatch match,
        ImportFootballMatchEventItem item,
        Dictionary<Guid, FootballTeam> teams,
        Dictionary<Guid, FootballPlayer> players,
        Dictionary<(Guid PlayerId, Guid TeamId, Guid SeasonId), FootballPlayerSeasonStatistics> playerStats,
        CancellationToken cancellationToken)
    {
        if (!item.PlayerId.HasValue)
        {
            throw new ArgumentException("Player is required for a card.");
        }

        if (!item.CardType.HasValue)
        {
            throw new ArgumentException("Card type is required for a card.");
        }

        FootballTeam team = await GetTeamAsync(item.TeamId, teams);
        FootballPlayer player = await GetPlayerAsync(item.PlayerId.Value, players);

        FootballCard card = match.RecordCard(
            team,
            player,
            item.CardType.Value,
            item.PeriodNumber,
            item.TimeInSeconds,
            item.Description);

        _matchRepository.MarkEventAsAdded(card);

        FootballPlayerSeasonStatistics stats = await GetOrCreatePlayerStatsAsync(
            playerStats, player.Id, item.TeamId, match.CompetitionId, cancellationToken);
        if (item.CardType.Value == FootballCardType.Yellow)
        {
            stats.RecordYellowCard();
        }
        else
        {
            stats.RecordRedCard();
        }
    }

    private async Task MutatePlayerStatsAsync(
        Dictionary<(Guid PlayerId, Guid TeamId, Guid SeasonId), FootballPlayerSeasonStatistics> cache,
        Guid playerId,
        Guid teamId,
        Guid seasonId,
        bool isGoal,
        bool isAssist,
        CancellationToken cancellationToken)
    {
        FootballPlayerSeasonStatistics stats = await GetOrCreatePlayerStatsAsync(
            cache, playerId, teamId, seasonId, cancellationToken);
        if (isGoal)
        {
            stats.RecordGoal();
        }

        if (isAssist)
        {
            stats.RecordAssist();
        }
    }

    private async Task<FootballPlayerSeasonStatistics> GetOrCreatePlayerStatsAsync(
        Dictionary<(Guid PlayerId, Guid TeamId, Guid SeasonId), FootballPlayerSeasonStatistics> cache,
        Guid playerId,
        Guid teamId,
        Guid seasonId,
        CancellationToken cancellationToken)
    {
        (Guid PlayerId, Guid TeamId, Guid SeasonId) key = (playerId, teamId, seasonId);
        if (cache.TryGetValue(key, out FootballPlayerSeasonStatistics? cached))
        {
            return cached;
        }

        FootballPlayerSeasonStatistics? existing =
            await _statisticsRepository.GetPlayerSeasonStatisticsAsync(playerId, teamId, seasonId, cancellationToken);
        FootballPlayerSeasonStatistics stats = existing ?? new FootballPlayerSeasonStatistics(playerId, teamId, seasonId);
        cache[key] = stats;
        return stats;
    }

    private async Task<FootballTeam> GetTeamAsync(Guid teamId, Dictionary<Guid, FootballTeam> cache)
    {
        if (cache.TryGetValue(teamId, out FootballTeam? cached))
        {
            return cached;
        }

        FootballTeam? team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            throw new InvalidOperationException($"Team with ID {teamId} not found.");
        }

        cache[teamId] = team;
        return team;
    }

    private async Task<FootballPlayer> GetPlayerAsync(Guid playerId, Dictionary<Guid, FootballPlayer> cache)
    {
        if (cache.TryGetValue(playerId, out FootballPlayer? cached))
        {
            return cached;
        }

        FootballPlayer? player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new InvalidOperationException($"Player with ID {playerId} not found.");
        }

        cache[playerId] = player;
        return player;
    }
}
