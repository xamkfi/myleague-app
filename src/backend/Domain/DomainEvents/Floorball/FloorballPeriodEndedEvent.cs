using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a period ends in a floorball match
/// </summary>
public class FloorballPeriodEndedEvent : FloorballDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the event
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the date and time when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; }

    /// <summary>
    /// Gets the number of the period that ended
    /// </summary>
    public int PeriodNumber { get; }

    /// <summary>
    /// Gets the home team's score at the end of the period
    /// </summary>
    public int HomeTeamScore { get; }

    /// <summary>
    /// Gets the away team's score at the end of the period
    /// </summary>
    public int AwayTeamScore { get; }

    /// <summary>
    /// Gets whether this was the last period of regular time
    /// </summary>
    public bool IsLastRegularPeriod { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballPeriodEndedEvent class
    /// </summary>
    public FloorballPeriodEndedEvent(
        Guid matchId,
        int periodNumber,
        int homeTeamScore,
        int awayTeamScore,
        bool isLastRegularPeriod)
    {
        if (periodNumber < 1)
        {
            throw new ArgumentException("Period number must be positive", nameof(periodNumber));
        }

        if (homeTeamScore < 0 || awayTeamScore < 0)
        {
            throw new ArgumentException("Scores cannot be negative", nameof(homeTeamScore));
        }

        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        PeriodNumber = periodNumber;
        HomeTeamScore = homeTeamScore;
        AwayTeamScore = awayTeamScore;
        IsLastRegularPeriod = isLastRegularPeriod;
    }

    /// <summary>
    /// Private constructor for EF Core serialization.
    /// </summary>
    private FloorballPeriodEndedEvent() { }
} 
