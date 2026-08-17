namespace Domain.Entities.Football.Matches;

/// <summary>
/// A substitution during a football match.
/// </summary>
public class FootballSubstitution : FootballMatchEvent
{
    public Guid PlayerOffId { get; private set; }
    public Guid PlayerOnId { get; private set; }

    private FootballSubstitution()
    {
    }

    public FootballSubstitution(
        Guid matchId,
        Guid teamId,
        Guid playerOffId,
        Guid playerOnId,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
        : base(matchId, teamId, periodNumber, timeInSeconds, description)
    {
        if (playerOffId == Guid.Empty)
            throw new ArgumentException("Player going off cannot be empty.", nameof(playerOffId));
        if (playerOnId == Guid.Empty)
            throw new ArgumentException("Player coming on cannot be empty.", nameof(playerOnId));
        if (playerOffId == playerOnId)
            throw new ArgumentException("A player cannot substitute themselves.", nameof(playerOnId));

        PlayerOffId = playerOffId;
        PlayerOnId = playerOnId;
    }
}
