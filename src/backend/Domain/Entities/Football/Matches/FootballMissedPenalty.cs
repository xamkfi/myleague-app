namespace Domain.Entities.Football.Matches;

/// <summary>
/// A penalty kick that did not result in a goal. It does not affect the score.
/// </summary>
public class FootballMissedPenalty : FootballMatchEvent
{
    /// <summary>
    /// The player who took the kick.
    /// </summary>
    public Guid ShooterPlayerId { get; private set; }

    private FootballMissedPenalty()
    {
    }

    public FootballMissedPenalty(
        Guid matchId,
        Guid teamId,
        Guid shooterPlayerId,
        int periodNumber,
        int timeInSeconds)
        : base(matchId, teamId, periodNumber, timeInSeconds)
    {
        if (shooterPlayerId == Guid.Empty)
            throw new ArgumentException("Shooter is required.", nameof(shooterPlayerId));

        ShooterPlayerId = shooterPlayerId;
    }
}
