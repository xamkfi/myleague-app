using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

public class HockeyShootoutAttempt : HockeyMatchEvent
{
    public Guid ShooterActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer Shooter { get; private set; } = null!;

    public Guid GoalieActivePlayerId { get; private set; }
    public HockeyMatchActivePlayer Goalie { get; private set; } = null!;

    public int ShotOrder { get; private set; }
    public HockeyShootoutAttemptResult Result { get; private set; }

    private HockeyShootoutAttempt() { }

    public HockeyShootoutAttempt(
        Guid matchId,
        Guid matchTeamId,
        Guid shooterActivePlayerId,
        Guid goalieActivePlayerId,
        int periodNumber,
        TimeSpan gameTime,
        int shotOrder,
        HockeyShootoutAttemptResult result,
        string? description = null)
        : base(
            matchId,
            HockeyMatchEventType.ShootoutAttempt,
            periodNumber,
            gameTime,
            matchTeamId: matchTeamId,
            matchActivePlayerId: shooterActivePlayerId,
            description: description)
    {
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (shooterActivePlayerId == Guid.Empty)
            throw new ArgumentException("Shooter active player id cannot be empty.", nameof(shooterActivePlayerId));
        if (goalieActivePlayerId == Guid.Empty)
            throw new ArgumentException("Goalie active player id cannot be empty.", nameof(goalieActivePlayerId));
        if (shotOrder < 1)
            throw new ArgumentOutOfRangeException(nameof(shotOrder), "Shot order must be at least 1.");

        ShooterActivePlayerId = shooterActivePlayerId;
        GoalieActivePlayerId = goalieActivePlayerId;
        ShotOrder = shotOrder;
        Result = result;
    }
}
