using System;

namespace Domain.ValueObjects.Floorball;

/// <summary>
/// Base abstract class for all floorball match events
/// </summary>
public abstract class FloorballMatchEventBase
{
    /// <summary>
    /// Gets the unique identifier of the event
    /// </summary>
    public Guid Id { get; protected set; }
    
    /// <summary>
    /// Gets the ID of the match this event belongs to
    /// </summary>
    public Guid MatchId { get; protected set; }
    
    /// <summary>
    /// Gets the ID of the team involved in the event
    /// </summary>
    public Guid TeamId { get; protected set; }
    
    /// <summary>
    /// Gets the period number when the event occurred
    /// </summary>
    public int PeriodNumber { get; protected set; }
    
    /// <summary>
    /// Gets the time in seconds when the event occurred in the period
    /// </summary>
    public int TimeInSeconds { get; protected set; }
    
    /// <summary>
    /// Gets the description of the event
    /// </summary>
    public string Description { get; protected set; }
    
    /// <summary>
    /// Protected constructor for EF Core and derived classes
    /// </summary>
    protected FloorballMatchEventBase()
    {
        Id = Guid.NewGuid();
    }
    
    /// <summary>
    /// Initializes a new instance of the FloorballMatchEventBase class
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="teamId">The ID of the team</param>
    /// <param name="periodNumber">The period number</param>
    /// <param name="timeInSeconds">The time in seconds</param>
    /// <param name="description">The description of the event</param>
    protected FloorballMatchEventBase(
        Guid matchId,
        Guid teamId,
        int periodNumber,
        int timeInSeconds,
        string description = null)
    {
        Id = Guid.NewGuid();
        MatchId = matchId;
        TeamId = teamId;
        PeriodNumber = periodNumber;
        TimeInSeconds = timeInSeconds;
        Description = description;
    }
    
    /// <summary>
    /// Gets the formatted time string (MM:SS)
    /// </summary>
    /// <returns>The formatted time string</returns>
    public string GetFormattedTime()
    {
        int minutes = TimeInSeconds / 60;
        int seconds = TimeInSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }
} 
