using Domain.Enums.Hockey.Matches;

namespace Domain.Entities.Hockey.Matches.Events;

/// <summary>
/// A penalty shot that did not result in a goal. The shooter is the event's primary active player.
/// It does not affect the score, shots or saves.
/// </summary>
public class HockeyMissedPenaltyShot : HockeyMatchEvent
{
    private HockeyMissedPenaltyShot() { }

    public HockeyMissedPenaltyShot(
        Guid matchId,
        Guid matchTeamId,
        Guid shooterActivePlayerId,
        int periodNumber,
        TimeSpan gameTime)
        : base(
            matchId,
            HockeyMatchEventType.MissedPenaltyShot,
            periodNumber,
            gameTime,
            matchTeamId: matchTeamId,
            matchActivePlayerId: shooterActivePlayerId)
    {
        if (matchTeamId == Guid.Empty)
            throw new ArgumentException("Match team id cannot be empty.", nameof(matchTeamId));
        if (shooterActivePlayerId == Guid.Empty)
            throw new ArgumentException("Shooter active player id cannot be empty.", nameof(shooterActivePlayerId));
    }
}
