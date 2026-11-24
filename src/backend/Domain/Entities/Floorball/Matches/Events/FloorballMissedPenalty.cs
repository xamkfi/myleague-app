namespace Domain.Entities.Floorball.Matches.Events;

/// <summary>
/// Represents a penalty shot that did not result in a goal. It does not affect the score, shots or saves.
/// </summary>
public class FloorballMissedPenalty : FloorballMatchEvent
{
    /// <summary>
    /// Gets the ID of the player who took the penalty shot
    /// </summary>
    public Guid ShooterPlayerId { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballMissedPenalty() : base() { }

    /// <summary>
    /// Initializes a new instance of the FloorballMissedPenalty class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the shooting team</param>
    /// <param name="shooterPlayerId">The ID of the player who took the shot</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds</param>
    public FloorballMissedPenalty(
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
