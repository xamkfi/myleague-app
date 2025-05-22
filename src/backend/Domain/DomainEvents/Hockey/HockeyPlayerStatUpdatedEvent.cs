using Domain.DomainEvents;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Enum representing the type of stat update
/// </summary>
public enum StatUpdateType
{
    /// <summary>
    /// Goal stat update
    /// </summary>
    Goal,
    
    /// <summary>
    /// Assist stat update
    /// </summary>
    Assist
}

/// <summary>
/// Event raised when a hockey player's stats are updated
/// </summary>
public class HockeyPlayerStatUpdatedEvent : IDomainEvent
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
    /// Gets the ID of the player
    /// </summary>
    public Guid PlayerId { get; }

    /// <summary>
    /// Gets the updated career goals count
    /// </summary>
    public int CareerGoals { get; }

    /// <summary>
    /// Gets the updated career assists count
    /// </summary>
    public int CareerAssists { get; }

    /// <summary>
    /// Gets the type of stat update
    /// </summary>
    public StatUpdateType UpdateType { get; }

    /// <summary>
    /// Initializes a new instance of the HockeyPlayerStatUpdatedEvent class
    /// </summary>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="careerGoals">The updated career goals count</param>
    /// <param name="careerAssists">The updated career assists count</param>
    /// <param name="updateType">The type of stat update</param>
    public HockeyPlayerStatUpdatedEvent(Guid playerId, int careerGoals, int careerAssists, StatUpdateType updateType)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        PlayerId = playerId;
        CareerGoals = careerGoals;
        CareerAssists = careerAssists;
        UpdateType = updateType;
    }
} 