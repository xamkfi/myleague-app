using System;

namespace Domain2.ValueObjects.Floorball;

/// <summary>
/// Base class for all match event value objects
/// </summary>
public abstract class MatchEventBaseValue
{
    /// <summary>
    /// Gets the ID of the match
    /// </summary>
    public Guid MatchId { get; private set; }
    
    /// <summary>
    /// Gets the ID of the team
    /// </summary>
    public Guid TeamId { get; private set; }
    
    /// <summary>
    /// Gets the period number
    /// </summary>
    public int PeriodNumber { get; private set; }
    
    /// <summary>
    /// Gets the time in seconds when the event occurred
    /// </summary>
    public int TimeInSeconds { get; private set; }
    
    /// <summary>
    /// Gets the description of the event
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Gets the formatted time string (MM:SS)
    /// </summary>
    public string FormattedTime
    {
        get
        {
            int minutes = TimeInSeconds / 60;
            int seconds = TimeInSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected MatchEventBaseValue()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the MatchEventBaseValue class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the team</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds</param>
    /// <param name="description">The description of the event</param>
    protected MatchEventBaseValue(
        Guid matchId,
        Guid teamId,
        int periodNumber,
        int timeInSeconds,
        string? description = null)
    {
        if (periodNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be positive.");
        if (timeInSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeInSeconds), "Time cannot be negative.");
        
        MatchId = matchId;
        TeamId = teamId;
        PeriodNumber = periodNumber;
        TimeInSeconds = timeInSeconds;
        Description = description ?? string.Empty;
    }
} 