using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when match statistics are updated
/// </summary>
public class FloorballMatchStatsUpdatedEvent : IDomainEvent
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
    /// Gets the current period number
    /// </summary>
    public int PeriodNumber { get; }

    /// <summary>
    /// Gets the time in seconds of the current period
    /// </summary>
    public int TimeInSeconds { get; }

    /// <summary>
    /// Gets the home team's score
    /// </summary>
    public int HomeTeamScore { get; }

    /// <summary>
    /// Gets the away team's score
    /// </summary>
    public int AwayTeamScore { get; }

    /// <summary>
    /// Gets the home team's shots on goal
    /// </summary>
    public int HomeTeamShots { get; }

    /// <summary>
    /// Gets the away team's shots on goal
    /// </summary>
    public int AwayTeamShots { get; }

    /// <summary>
    /// Gets whether the match is in overtime
    /// </summary>
    public bool IsOvertime { get; }

    /// <summary>
    /// Gets whether the match is in shootout
    /// </summary>
    public bool IsShootout { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballMatchStatsUpdatedEvent class
    /// </summary>
    public FloorballMatchStatsUpdatedEvent(
        Guid matchId,
        int periodNumber,
        int timeInSeconds,
        int homeTeamScore,
        int awayTeamScore,
        int homeTeamShots,
        int awayTeamShots,
        bool isOvertime = false,
        bool isShootout = false)
    {
        if (periodNumber < 1)
        {
            throw new ArgumentException("Period number must be positive", nameof(periodNumber));
        }

        if (timeInSeconds < 0 || timeInSeconds > 1200)
        {
            throw new ArgumentException("Time must be between 0 and 1200 seconds", nameof(timeInSeconds));
        }

        if (homeTeamScore < 0 || awayTeamScore < 0)
        {
            throw new ArgumentException("Scores cannot be negative", nameof(homeTeamScore));
        }

        if (homeTeamShots < 0 || awayTeamShots < 0)
        {
            throw new ArgumentException("Shots cannot be negative", nameof(homeTeamShots));
        }

        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        MatchId = matchId;
        PeriodNumber = periodNumber;
        TimeInSeconds = timeInSeconds;
        HomeTeamScore = homeTeamScore;
        AwayTeamScore = awayTeamScore;
        HomeTeamShots = homeTeamShots;
        AwayTeamShots = awayTeamShots;
        IsOvertime = isOvertime;
        IsShootout = isShootout;
    }
} 
