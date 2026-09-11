using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;

namespace Domain.Entities.Hockey.Statistics;

/// <summary>
/// Per-match team box score. Derived later from match events; not the live source of truth.
/// </summary>
public class HockeyMatchTeamStatistics : BaseEntity
{
    public Guid MatchId { get; private set; }
    public HockeyMatch Match { get; private set; } = null!;

    public Guid MatchTeamId { get; private set; }
    public HockeyMatchTeam MatchTeam { get; private set; } = null!;

    public Guid TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int ShotsOnGoal { get; private set; }
    public int ShotAttempts { get; private set; }
    public int MissedShots { get; private set; }
    public int BlockedShotAttempts { get; private set; }
    public decimal ShotPercentage { get; private set; }

    public int Saves { get; private set; }
    public int ShotsAgainst { get; private set; }
    public decimal TeamSavePercentage { get; private set; }

    public int FaceoffWins { get; private set; }
    public int FaceoffAttempts { get; private set; }
    public decimal FaceoffPercentage { get; private set; }

    public int PowerPlayOpportunities { get; private set; }
    public int PowerPlayGoals { get; private set; }
    public decimal PowerPlayPercentage { get; private set; }

    public int PenaltyKillOpportunities { get; private set; }
    public int PenaltyKillSuccesses { get; private set; }
    public decimal PenaltyKillPercentage { get; private set; }

    public int Penalties { get; private set; }
    public int PenaltyMinutes { get; private set; }
    public int Hits { get; private set; }
    public int BlockedShots { get; private set; }
    public int Takeaways { get; private set; }
    public int Giveaways { get; private set; }

    private HockeyMatchTeamStatistics() { }

    public HockeyMatchTeamStatistics(Guid matchId, Guid matchTeamId, Guid teamId)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));

        MatchId = matchId;
        MatchTeamId = matchTeamId;
        TeamId = teamId;
        RecalculatePercentages();
    }

    public void UpdateScoring(int goalsFor, int goalsAgainst)
    {
        HockeyStatisticsMath.EnsureNonNegative(goalsFor, nameof(goalsFor));
        HockeyStatisticsMath.EnsureNonNegative(goalsAgainst, nameof(goalsAgainst));
        GoalsFor = goalsFor;
        GoalsAgainst = goalsAgainst;
    }

    public void UpdateShooting(
        int shotsOnGoal,
        int shotAttempts,
        int missedShots,
        int blockedShotAttempts)
    {
        HockeyStatisticsMath.EnsureNonNegative(shotsOnGoal, nameof(shotsOnGoal));
        HockeyStatisticsMath.EnsureNonNegative(shotAttempts, nameof(shotAttempts));
        HockeyStatisticsMath.EnsureNonNegative(missedShots, nameof(missedShots));
        HockeyStatisticsMath.EnsureNonNegative(blockedShotAttempts, nameof(blockedShotAttempts));
        ShotsOnGoal = shotsOnGoal;
        ShotAttempts = shotAttempts;
        MissedShots = missedShots;
        BlockedShotAttempts = blockedShotAttempts;
        RecalculatePercentages();
    }

    public void UpdateGoaltending(int saves, int shotsAgainst)
    {
        HockeyStatisticsMath.EnsureNonNegative(saves, nameof(saves));
        HockeyStatisticsMath.EnsureNonNegative(shotsAgainst, nameof(shotsAgainst));
        Saves = saves;
        ShotsAgainst = shotsAgainst;
        RecalculatePercentages();
    }

    public void UpdateFaceoffs(int faceoffWins, int faceoffAttempts)
    {
        HockeyStatisticsMath.EnsureNonNegative(faceoffWins, nameof(faceoffWins));
        HockeyStatisticsMath.EnsureNonNegative(faceoffAttempts, nameof(faceoffAttempts));
        FaceoffWins = faceoffWins;
        FaceoffAttempts = faceoffAttempts;
        RecalculatePercentages();
    }

    public void UpdateSpecialTeams(
        int powerPlayOpportunities,
        int powerPlayGoals,
        int penaltyKillOpportunities,
        int penaltyKillSuccesses)
    {
        HockeyStatisticsMath.EnsureNonNegative(powerPlayOpportunities, nameof(powerPlayOpportunities));
        HockeyStatisticsMath.EnsureNonNegative(powerPlayGoals, nameof(powerPlayGoals));
        HockeyStatisticsMath.EnsureNonNegative(penaltyKillOpportunities, nameof(penaltyKillOpportunities));
        HockeyStatisticsMath.EnsureNonNegative(penaltyKillSuccesses, nameof(penaltyKillSuccesses));
        PowerPlayOpportunities = powerPlayOpportunities;
        PowerPlayGoals = powerPlayGoals;
        PenaltyKillOpportunities = penaltyKillOpportunities;
        PenaltyKillSuccesses = penaltyKillSuccesses;
        RecalculatePercentages();
    }

    public void UpdateDisciplineAndMisc(
        int penalties,
        int penaltyMinutes,
        int hits,
        int blockedShots,
        int takeaways,
        int giveaways)
    {
        HockeyStatisticsMath.EnsureNonNegative(penalties, nameof(penalties));
        HockeyStatisticsMath.EnsureNonNegative(penaltyMinutes, nameof(penaltyMinutes));
        HockeyStatisticsMath.EnsureNonNegative(hits, nameof(hits));
        HockeyStatisticsMath.EnsureNonNegative(blockedShots, nameof(blockedShots));
        HockeyStatisticsMath.EnsureNonNegative(takeaways, nameof(takeaways));
        HockeyStatisticsMath.EnsureNonNegative(giveaways, nameof(giveaways));
        Penalties = penalties;
        PenaltyMinutes = penaltyMinutes;
        Hits = hits;
        BlockedShots = blockedShots;
        Takeaways = takeaways;
        Giveaways = giveaways;
    }

    private void RecalculatePercentages()
    {
        ShotPercentage = HockeyStatisticsMath.Percentage(GoalsFor, ShotsOnGoal);
        TeamSavePercentage = HockeyStatisticsMath.Percentage(Saves, ShotsAgainst);
        FaceoffPercentage = HockeyStatisticsMath.Percentage(FaceoffWins, FaceoffAttempts);
        PowerPlayPercentage = HockeyStatisticsMath.Percentage(PowerPlayGoals, PowerPlayOpportunities);
        PenaltyKillPercentage = HockeyStatisticsMath.Percentage(PenaltyKillSuccesses, PenaltyKillOpportunities);
    }
}
