using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Statistics;
using Domain.Entities.Floorball.Teams;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using Domain.ValueObjects.Floorball;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Statistics updates shared by the floorball match handlers, so recording, correcting, reopening
/// and deleting a match always apply and revert the same numbers.
/// </summary>
internal static class FloorballMatchStatisticsSupport
{
    /// <summary>
    /// Sets both teams' shot totals from the match events.
    /// </summary>
    public static async Task SyncMatchShotsAsync(
        IFloorballStatisticsRepository repository,
        FloorballMatch match,
        CancellationToken cancellationToken)
    {
        foreach (Guid teamId in TeamIds(match))
        {
            FloorballMatchTeamStatistics stats =
                await repository.GetMatchTeamStatisticsAsync(match.Id, teamId, cancellationToken)
                ?? new FloorballMatchTeamStatistics(match.Id, teamId);
            FloorballTeamShots shots = match.CalculateTeamShots(teamId);
            stats.SetShotStatistics(shots.OnGoal, shots.Total);
            await repository.SaveMatchTeamStatisticsAsync(stats, cancellationToken);
        }
    }

    /// <summary>
    /// Adds saves and conceded goals to a goalie's season totals. Negative counts remove them.
    /// </summary>
    public static async Task AdjustGoalieAsync(
        IFloorballStatisticsRepository repository,
        Guid goalieId,
        Guid teamId,
        Guid competitionId,
        int saves,
        int goalsAgainst,
        CancellationToken cancellationToken)
    {
        if (saves == 0 && goalsAgainst == 0)
            return;

        FloorballGoalieSeasonStatistics? stats =
            await repository.GetGoalieSeasonStatisticsAsync(goalieId, teamId, competitionId, cancellationToken);
        if (stats == null)
        {
            if (saves < 0 || goalsAgainst < 0)
                return;

            stats = new FloorballGoalieSeasonStatistics(goalieId, teamId, competitionId);
        }

        stats.RecordSaves(saves, saves + goalsAgainst, goalsAgainst);
        await repository.SaveGoalieSeasonStatisticsAsync(stats, cancellationToken);
    }

    /// <summary>
    /// Reverts what recording a goal added to player, team and goalie season totals.
    /// </summary>
    public static async Task UndoGoalAsync(
        IFloorballStatisticsRepository repository,
        IFloorballPlayerRepository playerRepository,
        FloorballMatch match,
        FloorballGoal goal,
        CancellationToken cancellationToken)
    {
        // Own goals were never credited to a player.
        if (goal.GoalType != FloorballGoalType.OwnGoal)
        {
            await UndoPlayerPointAsync(repository, playerRepository, match, goal.TeamId, goal.ScoringPlayerId, isGoal: true, cancellationToken);
            await UndoPlayerPointAsync(repository, playerRepository, match, goal.TeamId, goal.AssistingPlayerId, isGoal: false, cancellationToken);
            await UndoPlayerPointAsync(repository, playerRepository, match, goal.TeamId, goal.SecondaryAssistingPlayerId, isGoal: false, cancellationToken);
        }

        FloorballTeamSeasonStatistics? scoringTeam =
            await repository.GetTeamSeasonStatisticsAsync(goal.TeamId, match.CompetitionId, cancellationToken);
        if (scoringTeam != null)
        {
            scoringTeam.DecrementGoalsFor();
            await repository.SaveTeamSeasonStatisticsAsync(scoringTeam, cancellationToken);
        }

        Guid opposingTeamId = OpposingTeamId(match, goal.TeamId);
        FloorballTeamSeasonStatistics? opposingTeam =
            await repository.GetTeamSeasonStatisticsAsync(opposingTeamId, match.CompetitionId, cancellationToken);
        if (opposingTeam != null)
        {
            opposingTeam.DecrementGoalsAgainst();
            await repository.SaveTeamSeasonStatisticsAsync(opposingTeam, cancellationToken);
        }

        if (goal.GoalieId.HasValue)
        {
            await AdjustGoalieAsync(
                repository, goal.GoalieId.Value, opposingTeamId, match.CompetitionId, saves: 0, goalsAgainst: -1, cancellationToken);
        }
    }

    /// <summary>
    /// Reverts the penalty minutes of a penalty from the player's season totals.
    /// </summary>
    public static async Task UndoPenaltyAsync(
        IFloorballStatisticsRepository repository,
        FloorballMatch match,
        FloorballPenalty penalty,
        CancellationToken cancellationToken)
    {
        if (!penalty.PlayerId.HasValue)
            return;

        FloorballPlayerSeasonStatistics? stats = await repository.GetPlayerSeasonStatisticsAsync(
            penalty.PlayerId.Value, penalty.TeamId, match.CompetitionId, cancellationToken);
        if (stats == null)
            return;

        stats.RemovePenaltyMinutes(penalty.DurationInMinutes);
        await repository.SavePlayerSeasonStatisticsAsync(stats, cancellationToken);
    }

    /// <summary>
    /// Adds the completed match to the season totals of the goalies who were in goal. The active
    /// goalie gets the result; the others only get the appearance.
    /// </summary>
    public static async Task ApplyGoalieCompletionAsync(
        IFloorballStatisticsRepository repository,
        FloorballMatch match,
        CancellationToken cancellationToken)
    {
        foreach (Guid teamId in TeamIds(match))
        {
            Guid? activeGoalieId = match.GetActiveGoalieId(teamId);
            foreach (Guid goalieId in match.GetGoaliesInGoal(teamId))
            {
                FloorballGoalieSeasonStatistics stats =
                    await repository.GetGoalieSeasonStatisticsAsync(goalieId, teamId, match.CompetitionId, cancellationToken)
                    ?? new FloorballGoalieSeasonStatistics(goalieId, teamId, match.CompetitionId);

                if (goalieId == activeGoalieId)
                {
                    stats.RecordGamePlayed(
                        wasStarter: true, ResultOf(match, teamId), MinutesOf(match), OpponentScore(match, teamId) == 0);
                }
                else
                {
                    stats.RecordAppearance();
                }

                await repository.SaveGoalieSeasonStatisticsAsync(stats, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Reverts everything completing the match added: team results, games played, and goalie results.
    /// </summary>
    public static async Task UndoCompletionAsync(
        IFloorballStatisticsRepository repository,
        FloorballMatch match,
        CancellationToken cancellationToken)
    {
        FloorballStandingRules? standingRules = match.Competition is FloorballSeason season ? season.StandingRules : null;

        foreach (Guid teamId in TeamIds(match))
        {
            FloorballTeamSeasonStatistics? teamStats =
                await repository.GetTeamSeasonStatisticsAsync(teamId, match.CompetitionId, cancellationToken);
            if (teamStats == null)
                continue;

            teamStats.UndoMatchResult(
                ResultOf(match, teamId), teamId == match.HomeTeamId, match.WentToShootout, match.WentToOvertime, standingRules);
            await repository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
        }

        foreach ((Guid playerId, Guid teamId) in CompleteFloorballMatchHandler.CollectMatchParticipants(match))
        {
            FloorballPlayerSeasonStatistics? playerStats =
                await repository.GetPlayerSeasonStatisticsAsync(playerId, teamId, match.CompetitionId, cancellationToken);
            if (playerStats == null)
                continue;

            playerStats.RemoveGamePlayed();
            await repository.SavePlayerSeasonStatisticsAsync(playerStats, cancellationToken);
        }

        foreach (Guid teamId in TeamIds(match))
        {
            Guid? activeGoalieId = match.GetActiveGoalieId(teamId);
            foreach (Guid goalieId in match.GetGoaliesInGoal(teamId))
            {
                FloorballGoalieSeasonStatistics? goalieStats =
                    await repository.GetGoalieSeasonStatisticsAsync(goalieId, teamId, match.CompetitionId, cancellationToken);
                if (goalieStats == null)
                    continue;

                if (goalieId == activeGoalieId)
                {
                    goalieStats.UndoGamePlayed(
                        wasStarter: true, ResultOf(match, teamId), MinutesOf(match), OpponentScore(match, teamId) == 0);
                }
                else
                {
                    goalieStats.UndoAppearance();
                }

                await repository.SaveGoalieSeasonStatisticsAsync(goalieStats, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Reverts every statistic the match has produced, before the match itself is deleted.
    /// </summary>
    public static async Task UndoMatchAsync(
        IFloorballStatisticsRepository repository,
        IFloorballPlayerRepository playerRepository,
        FloorballMatch match,
        CancellationToken cancellationToken)
    {
        if (match.Status == FloorballMatchStatus.Completed)
            await UndoCompletionAsync(repository, match, cancellationToken);

        foreach (FloorballGoal goal in match.GoalEvents)
        {
            await UndoGoalAsync(repository, playerRepository, match, goal, cancellationToken);
        }

        foreach (FloorballPenalty penalty in match.PenaltyEvents)
        {
            await UndoPenaltyAsync(repository, match, penalty, cancellationToken);
        }

        foreach (FloorballSave save in match.SaveEvents)
        {
            await AdjustGoalieAsync(
                repository, save.GoalieId, save.TeamId, match.CompetitionId, saves: -1, goalsAgainst: 0, cancellationToken);
        }

        await repository.RemoveMatchTeamStatisticsAsync(match.Id, cancellationToken);
    }

    /// <summary>
    /// Gets the team playing against <paramref name="teamId"/>. A started match has both teams.
    /// </summary>
    public static Guid OpposingTeamId(FloorballMatch match, Guid teamId) =>
        (teamId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId)!.Value;

    private static async Task UndoPlayerPointAsync(
        IFloorballStatisticsRepository repository,
        IFloorballPlayerRepository playerRepository,
        FloorballMatch match,
        Guid teamId,
        Guid? playerId,
        bool isGoal,
        CancellationToken cancellationToken)
    {
        if (!playerId.HasValue)
            return;

        FloorballPlayer? player = await playerRepository.GetByIdAsync(playerId.Value);
        FloorballPlayerSeasonStatistics? stats = await repository.GetPlayerSeasonStatisticsAsync(
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

    private static IEnumerable<Guid> TeamIds(FloorballMatch match) =>
        new[] { match.HomeTeamId, match.AwayTeamId }.OfType<Guid>();

    private static int MinutesOf(FloorballMatch match) =>
        match.MatchRules.PeriodDurationMinutes * match.MatchRules.NumberOfPeriods;

    private static int OpponentScore(FloorballMatch match, Guid teamId) =>
        teamId == match.HomeTeamId ? match.AwayScore : match.HomeScore;

    private static FloorballGameResult ResultOf(FloorballMatch match, Guid teamId)
    {
        int ownScore = teamId == match.HomeTeamId ? match.HomeScore : match.AwayScore;
        int opponentScore = OpponentScore(match, teamId);

        return ownScore > opponentScore
            ? FloorballGameResult.Win
            : ownScore < opponentScore ? FloorballGameResult.Loss : FloorballGameResult.Tie;
    }
}
