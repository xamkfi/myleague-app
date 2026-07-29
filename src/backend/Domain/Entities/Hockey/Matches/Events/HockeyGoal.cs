using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyGoal : HockeyMatchEvent
{
    public Guid ScoringMatchTeamId { get; private set; }
    public HockeyMatchTeam ScoringMatchTeam { get; private set; } = null!;

    public Guid ScorerActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer Scorer { get; private set; } = null!;

    public Guid? PrimaryAssistActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? PrimaryAssist { get; private set; }

    public Guid? SecondaryAssistActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? SecondaryAssist { get; private set; }

    public Guid? GoalieActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer? Goalie { get; private set; }

    public Guid? RelatedShotId { get; private set; }
    public HockeyShot? RelatedShot { get; private set; }

    public HockeyGoalStrength GoalStrength { get; private set; }
    public bool IsGameWinningGoal { get; private set; }
    public bool WasEmptyNet { get; private set; }
    public bool WasDelayedPenalty { get; private set; }
    public bool WasPenaltyShotGoal { get; private set; }

    private HockeyGoal() { }

    public HockeyGoal(
        Guid matchId,
        Guid scoringMatchTeamId,
        Guid scorerActivePlayerId,
        int periodNumber,
        TimeSpan gameTime,
        HockeyGoalStrength goalStrength,
        Guid? primaryAssistActivePlayerId = null,
        Guid? secondaryAssistActivePlayerId = null,
        Guid? goalieActivePlayerId = null,
        Guid? relatedShotId = null,
        bool isGameWinningGoal = false,
        bool wasEmptyNet = false,
        bool wasDelayedPenalty = false,
        bool wasPenaltyShotGoal = false,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.Goal,
            periodNumber,
            gameTime,
            matchTeamId: scoringMatchTeamId,
            matchActivePlayerId: scorerActivePlayerId,
            description: description)
    {
        if (scoringMatchTeamId == Guid.Empty)
            throw new ArgumentException("Scoring match team id cannot be empty.", nameof(scoringMatchTeamId));
        if (scorerActivePlayerId == Guid.Empty)
            throw new ArgumentException("Scorer active player id cannot be empty.", nameof(scorerActivePlayerId));
        if (primaryAssistActivePlayerId == Guid.Empty)
            throw new ArgumentException("Primary assist id cannot be empty.", nameof(primaryAssistActivePlayerId));
        if (secondaryAssistActivePlayerId == Guid.Empty)
            throw new ArgumentException("Secondary assist id cannot be empty.", nameof(secondaryAssistActivePlayerId));
        if (goalieActivePlayerId == Guid.Empty)
            throw new ArgumentException("Goalie active player id cannot be empty.", nameof(goalieActivePlayerId));
        if (relatedShotId == Guid.Empty)
            throw new ArgumentException("Related shot id cannot be empty.", nameof(relatedShotId));

        ScoringMatchTeamId = scoringMatchTeamId;
        ScorerActivePlayerId = scorerActivePlayerId;
        PrimaryAssistActivePlayerId = primaryAssistActivePlayerId;
        SecondaryAssistActivePlayerId = secondaryAssistActivePlayerId;
        GoalieActivePlayerId = goalieActivePlayerId;
        RelatedShotId = relatedShotId;
        GoalStrength = goalStrength;
        IsGameWinningGoal = isGameWinningGoal;
        WasEmptyNet = wasEmptyNet;
        WasDelayedPenalty = wasDelayedPenalty;
        WasPenaltyShotGoal = wasPenaltyShotGoal;
    }

    public void LinkRelatedShot(HockeyShot shot)
    {
        ArgumentNullException.ThrowIfNull(shot);
        if (shot.MatchId != MatchId)
            throw new InvalidOperationException("Related shot must belong to the same match.");
        RelatedShotId = shot.Id;
        RelatedShot = shot;
    }
}
