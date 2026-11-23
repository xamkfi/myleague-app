using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Statistics;
using Domain.Entities.Football.Teams;
using Domain.Enums.Football;
using Domain.Repositories.Football;
using Domain.ValueObjects.Football;

namespace Application.Features.Football.Matches.Handlers;

/// <summary>
/// Reverts the statistics a football match has produced, for reopening, correcting and deleting it.
/// </summary>
internal static class FootballMatchStatisticsSupport
{
    /// <summary>
    /// Reverts what recording a goal added to the scorer and the assister.
    /// </summary>
    public static async Task UndoGoalAsync(
        IFootballStatisticsRepository repository,
        IFootballPlayerRepository playerRepository,
        FootballMatch match,
        FootballGoal goal,
        CancellationToken cancellationToken)
    {
        // Own goals were never credited to a player.
        if (goal.IsOwnGoal)
            return;

        await UndoPlayerPointAsync(repository, playerRepository, match, goal.TeamId, goal.ScoringPlayerId, isGoal: true, cancellationToken);
        await UndoPlayerPointAsync(repository, playerRepository, match, goal.TeamId, goal.AssistingPlayerId, isGoal: false, cancellationToken);
    }

    /// <summary>
    /// Reverts everything completing the match added: team results and games played.
    /// </summary>
    public static async Task UndoCompletionAsync(
        IFootballStatisticsRepository repository,
        FootballMatch match,
        CancellationToken cancellationToken)
    {
        if (match.HomeTeamId.HasValue)
            await UndoTeamResultAsync(repository, match, match.HomeTeamId.Value, match.HomeScore, match.AwayScore, isHomeGame: true, cancellationToken);
        if (match.AwayTeamId.HasValue)
            await UndoTeamResultAsync(repository, match, match.AwayTeamId.Value, match.AwayScore, match.HomeScore, isHomeGame: false, cancellationToken);

        foreach ((Guid playerId, Guid teamId) in CompleteFootballMatchHandler.CollectMatchParticipants(match))
        {
            FootballPlayerSeasonStatistics? playerStats =
                await repository.GetPlayerSeasonStatisticsAsync(playerId, teamId, match.CompetitionId, cancellationToken);
            if (playerStats == null)
                continue;

            playerStats.RemoveGamePlayed();
            await repository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
        }
    }

    /// <summary>
    /// Reverts every statistic the match has produced, before the match itself is deleted.
    /// </summary>
    public static async Task UndoMatchAsync(
        IFootballStatisticsRepository repository,
        IFootballPlayerRepository playerRepository,
        FootballMatch match,
        CancellationToken cancellationToken)
    {
        if (match.Status == FootballMatchStatus.Completed)
            await UndoCompletionAsync(repository, match, cancellationToken);

        foreach (FootballGoal goal in match.GoalEvents)
        {
            await UndoGoalAsync(repository, playerRepository, match, goal, cancellationToken);
        }

        foreach (FootballCard card in match.CardEvents)
        {
            await RecordCardHandler.UpdatePlayerCardStatistics(
                card, match.CompetitionId, increment: false, repository, cancellationToken);
        }
    }

    private static async Task UndoTeamResultAsync(
        IFootballStatisticsRepository repository,
        FootballMatch match,
        Guid teamId,
        int teamScore,
        int opponentScore,
        bool isHomeGame,
        CancellationToken cancellationToken)
    {
        FootballTeamSeasonStatistics? teamStats =
            await repository.GetTeamSeasonStatisticsAsync(teamId, match.CompetitionId, cancellationToken);
        if (teamStats == null)
            return;

        FootballGameResult gameResult = teamScore > opponentScore
            ? FootballGameResult.Win
            : teamScore < opponentScore
                ? FootballGameResult.Loss
                : FootballGameResult.Draw;

        int yellowCards = match.CardEvents.Count(c =>
            c.TeamId == teamId && c.CardType == FootballCardType.Yellow);
        int redCards = match.CardEvents.Count(c =>
            c.TeamId == teamId && c.ResultsInSendingOff);

        teamStats.RevertAfterMatch(
            gameResult,
            isHomeGame,
            teamScore,
            opponentScore,
            match.Competition?.StandingRules ?? FootballStandingRules.Default(),
            yellowCards,
            redCards,
            match.WentToExtraTime || match.WentToPenaltyShootout);

        await repository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }

    private static async Task UndoPlayerPointAsync(
        IFootballStatisticsRepository repository,
        IFootballPlayerRepository playerRepository,
        FootballMatch match,
        Guid teamId,
        Guid? playerId,
        bool isGoal,
        CancellationToken cancellationToken)
    {
        if (!playerId.HasValue)
            return;

        FootballPlayer? player = await playerRepository.GetByIdAsync(playerId.Value);
        FootballPlayerSeasonStatistics? stats = await repository.GetPlayerSeasonStatisticsAsync(
            playerId.Value, teamId, match.CompetitionId, cancellationToken);

        if (isGoal)
        {
            player?.RemoveGoal();
            stats?.RemoveGoal();
        }
        else
        {
            player?.RemoveAssist();
            stats?.RemoveAssist();
        }

        if (stats != null)
            await repository.SavePlayerSeasonStatisticsAsync(stats, cancellationToken);
    }
}
