using Domain.Entities.Hockey.Matches;
using Domain.Entities.Hockey.Teams;

namespace Domain.Entities.Hockey.Statistics;

/// <summary>
/// Per-match skater box score keyed by match active player.
/// </summary>
public class HockeyMatchPlayerStatistics : BaseEntity
{
    public Guid MatchId { get; private set; }
    public HockeyMatch Match { get; private set; } = null!;

    public Guid MatchTeamId { get; private set; }
    public HockeyMatchTeam MatchTeam { get; private set; } = null!;

    public Guid MatchActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer MatchActivePlayer { get; private set; } = null!;

    public Guid TeamPlayerId { get; private set; }
    public HockeyTeamPlayer? TeamPlayer { get; private set; }

    public Guid PlayerId { get; private set; }
    public HockeyPlayer? Player { get; private set; }

    public Guid TeamId { get; private set; }
    public HockeyTeam? Team { get; private set; }

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

    private HockeyMatchPlayerStatistics() { }

    public HockeyMatchPlayerStatistics(
        Guid matchId,
        Guid matchTeamId,
        Guid matchActivePlayerId,
        Guid teamPlayerId,
        Guid playerId,
        Guid teamId)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (matchActivePlayerId == Guid.Empty)
            throw new ArgumentException("Match active player id cannot be empty.", nameof(matchActivePlayerId));
        if (teamPlayerId == Guid.Empty)
            throw new ArgumentException("Team player id cannot be empty.", nameof(teamPlayerId));
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player id cannot be empty.", nameof(playerId));
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team id cannot be empty.", nameof(teamId));

        MatchId = matchId;
        MatchTeamId = matchTeamId;
        MatchActivePlayerId = matchActivePlayerId;
        TeamPlayerId = teamPlayerId;
        PlayerId = playerId;
        TeamId = teamId;
        GamesPlayed = 1;
        RecalculateDerived();
    }

    public void UpdateScoring(int goals, int assists, int penaltyMinutes, int plusMinusRating)
    {
        HockeyStatisticsMath.EnsureNonNegative(goals, nameof(goals));
        HockeyStatisticsMath.EnsureNonNegative(assists, nameof(assists));
        HockeyStatisticsMath.EnsureNonNegative(penaltyMinutes, nameof(penaltyMinutes));
        Goals = goals;
        Assists = assists;
        PenaltyMinutes = penaltyMinutes;
        PlusMinusRating = plusMinusRating;
        RecalculateDerived();
    }

    public void UpdateShooting(int shotsOnGoal, int shotAttempts)
    {
        HockeyStatisticsMath.EnsureNonNegative(shotsOnGoal, nameof(shotsOnGoal));
        HockeyStatisticsMath.EnsureNonNegative(shotAttempts, nameof(shotAttempts));
        ShotsOnGoal = shotsOnGoal;
        ShotAttempts = shotAttempts;
        RecalculateDerived();
    }

    public void UpdateFaceoffs(int faceoffWins, int faceoffAttempts)
    {
        HockeyStatisticsMath.EnsureNonNegative(faceoffWins, nameof(faceoffWins));
        HockeyStatisticsMath.EnsureNonNegative(faceoffAttempts, nameof(faceoffAttempts));
        FaceoffWins = faceoffWins;
        FaceoffAttempts = faceoffAttempts;
        RecalculateDerived();
    }

    public void UpdateMisc(int hits, int blockedShots, int takeaways, int giveaways, int timeOnIceSeconds, int shifts)
    {
        HockeyStatisticsMath.EnsureNonNegative(hits, nameof(hits));
        HockeyStatisticsMath.EnsureNonNegative(blockedShots, nameof(blockedShots));
        HockeyStatisticsMath.EnsureNonNegative(takeaways, nameof(takeaways));
        HockeyStatisticsMath.EnsureNonNegative(giveaways, nameof(giveaways));
        HockeyStatisticsMath.EnsureNonNegative(timeOnIceSeconds, nameof(timeOnIceSeconds));
        HockeyStatisticsMath.EnsureNonNegative(shifts, nameof(shifts));
        Hits = hits;
        BlockedShots = blockedShots;
        Takeaways = takeaways;
        Giveaways = giveaways;
        TimeOnIceSeconds = timeOnIceSeconds;
        Shifts = shifts;
    }

    private void RecalculateDerived()
    {
        Points = Goals + Assists;
        ShotPercentage = HockeyStatisticsMath.Percentage(Goals, ShotsOnGoal);
        FaceoffPercentage = HockeyStatisticsMath.Percentage(FaceoffWins, FaceoffAttempts);
    }
}
