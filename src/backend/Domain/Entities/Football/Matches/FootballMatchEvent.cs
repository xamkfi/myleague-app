namespace Domain.Entities.Football.Matches;

/// <summary>
/// Base class for all football match events.
/// TimeInSeconds is elapsed time within the current period (half).
/// </summary>
public abstract class FootballMatchEvent : BaseEntity
{
    public Guid MatchId { get; protected set; }
    public Guid TeamId { get; protected set; }
    public int PeriodNumber { get; protected set; }
    public int TimeInSeconds { get; protected set; }
    public string? Description { get; protected set; }

    public string FormattedTime
    {
        get
        {
            int minutes = TimeInSeconds / 60;
            int seconds = TimeInSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }

    protected FootballMatchEvent()
    {
    }

    protected FootballMatchEvent(
        Guid matchId,
        Guid teamId,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
    {
        if (periodNumber <= 0)
            throw new ArgumentException("Period number must be positive.", nameof(periodNumber));
        if (timeInSeconds < 0)
            throw new ArgumentException("Time cannot be negative.", nameof(timeInSeconds));

        MatchId = matchId;
        TeamId = teamId;
        PeriodNumber = periodNumber;
        TimeInSeconds = timeInSeconds;
        Description = description;
    }
}
