using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Teams;
using Domain.Enums.Hockey.Statistics;

namespace Domain.Entities.Hockey.Statistics;

/// <summary>
/// Aggregated skater statistics for a competition (season or tournament) at a given scope.
/// </summary>
public class HockeyPlayerCompetitionStatistics : BaseEntity
{
    public Guid PlayerId { get; private set; }
    public HockeyPlayer? Player { get; private set; }

    public Guid TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

    public Guid TeamPlayerId { get; private set; }
    public HockeyTeamPlayer? TeamPlayer { get; private set; }

    public Guid CompetitionId { get; private set; }
    public HockeyCompetition? Competition { get; private set; }

    public HockeyStatisticsScope Scope { get; private set; }

    public Guid? CompetitionDivisionId { get; private set; }
    public HockeyCompetitionDivision? CompetitionDivision { get; private set; }

    public Guid? TournamentGroupId { get; private set; }
    public HockeyTournamentGroup? TournamentGroup { get; private set; }

    public Guid? PlayoffSeriesId { get; private set; }
    public HockeyPlayoffSeries? PlayoffSeries { get; private set; }

    public int GamesPlayed { get; private set; }
    public int Goals { get; private set; }
    public int Assists { get; private set; }
    public int Points { get; private set; }
    public int PenaltyMinutes { get; private set; }
    public int PlusMinusRating { get; private set; }
    public int ShotsOnGoal { get; private set; }
    public int ShotAttempts { get; private set; }
    public decimal ShotPercentage { get; private set; }
    public int FaceoffWins { get; private set; }
    public int FaceoffAttempts { get; private set; }
    public decimal FaceoffPercentage { get; private set; }
    public int Hits { get; private set; }
    public int BlockedShots { get; private set; }
    public int Takeaways { get; private set; }
    public int Giveaways { get; private set; }
    public int TimeOnIceSeconds { get; private set; }
    public int Shifts { get; private set; }

    private HockeyPlayerCompetitionStatistics() { }

    public HockeyPlayerCompetitionStatistics(
        Guid playerId,
        Guid teamId,
        Guid teamPlayerId,
        Guid competitionId,
        HockeyStatisticsScope scope,
        Guid? competitionDivisionId = null,
        Guid? tournamentGroupId = null,
        Guid? playoffSeriesId = null)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player id cannot be empty.", nameof(playerId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));
        if (teamPlayerId == Guid.Empty)
            throw new ArgumentException("Team player id cannot be empty.", nameof(teamPlayerId));
        if (competitionId == Guid.Empty)
            throw new ArgumentException("Competition id cannot be empty.", nameof(competitionId));

        HockeyStatisticsScopeValidator.Validate(scope, competitionDivisionId, tournamentGroupId, playoffSeriesId);

        PlayerId = playerId;
        TeamId = teamId;
        TeamPlayerId = teamPlayerId;
        CompetitionId = competitionId;
        Scope = scope;
        CompetitionDivisionId = competitionDivisionId;
        TournamentGroupId = tournamentGroupId;
        PlayoffSeriesId = playoffSeriesId;
        RecalculateDerived();
    }

    public void UpdateTotals(
        int gamesPlayed,
        int goals,
        int assists,
        int penaltyMinutes,
        int plusMinusRating,
        int shotsOnGoal,
        int shotAttempts,
        int faceoffWins,
        int faceoffAttempts,
        int hits,
        int blockedShots,
        int takeaways,
        int giveaways,
        int timeOnIceSeconds,
        int shifts)
    {
        HockeyStatisticsMath.EnsureNonNegative(gamesPlayed, nameof(gamesPlayed));
        HockeyStatisticsMath.EnsureNonNegative(goals, nameof(goals));
        HockeyStatisticsMath.EnsureNonNegative(assists, nameof(assists));
        HockeyStatisticsMath.EnsureNonNegative(penaltyMinutes, nameof(penaltyMinutes));
        HockeyStatisticsMath.EnsureNonNegative(shotsOnGoal, nameof(shotsOnGoal));
        HockeyStatisticsMath.EnsureNonNegative(shotAttempts, nameof(shotAttempts));
        HockeyStatisticsMath.EnsureNonNegative(faceoffWins, nameof(faceoffWins));
        HockeyStatisticsMath.EnsureNonNegative(faceoffAttempts, nameof(faceoffAttempts));
        HockeyStatisticsMath.EnsureNonNegative(hits, nameof(hits));
        HockeyStatisticsMath.EnsureNonNegative(blockedShots, nameof(blockedShots));
        HockeyStatisticsMath.EnsureNonNegative(takeaways, nameof(takeaways));
        HockeyStatisticsMath.EnsureNonNegative(giveaways, nameof(giveaways));
        HockeyStatisticsMath.EnsureNonNegative(timeOnIceSeconds, nameof(timeOnIceSeconds));
        HockeyStatisticsMath.EnsureNonNegative(shifts, nameof(shifts));

        GamesPlayed = gamesPlayed;
        Goals = goals;
        Assists = assists;
        PenaltyMinutes = penaltyMinutes;
        PlusMinusRating = plusMinusRating;
        ShotsOnGoal = shotsOnGoal;
        ShotAttempts = shotAttempts;
        FaceoffWins = faceoffWins;
        FaceoffAttempts = faceoffAttempts;
        Hits = hits;
        BlockedShots = blockedShots;
        Takeaways = takeaways;
        Giveaways = giveaways;
        TimeOnIceSeconds = timeOnIceSeconds;
        Shifts = shifts;
        RecalculateDerived();
    }

    private void RecalculateDerived()
    {
        Points = Goals + Assists;
        ShotPercentage = HockeyStatisticsMath.Percentage(Goals, ShotsOnGoal);
        FaceoffPercentage = HockeyStatisticsMath.Percentage(FaceoffWins, FaceoffAttempts);
    }
}
