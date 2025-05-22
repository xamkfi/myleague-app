using Domain.DomainEvents;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a hockey season is completed
/// </summary>
public class HockeySeasonCompletedEvent : IDomainEvent
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
    /// Gets the ID of the season
    /// </summary>
    public Guid SeasonId { get; }

    /// <summary>
    /// Initializes a new instance of the HockeySeasonCompletedEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    public HockeySeasonCompletedEvent(Guid seasonId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
    }
} 