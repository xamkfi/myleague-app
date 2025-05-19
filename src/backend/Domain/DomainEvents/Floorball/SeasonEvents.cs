using Domain.DomainEvents;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a season is activated
/// </summary>
public class SeasonActivatedDomainEvent : IDomainEvent
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
    /// Initializes a new instance of the SeasonActivatedDomainEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    public SeasonActivatedDomainEvent(Guid seasonId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
    }
}

/// <summary>
/// Event raised when a season is deactivated
/// </summary>
public class SeasonDeactivatedDomainEvent : IDomainEvent
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
    /// Initializes a new instance of the SeasonDeactivatedDomainEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    public SeasonDeactivatedDomainEvent(Guid seasonId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
    }
}

/// <summary>
/// Event raised when a season is completed
/// </summary>
public class SeasonCompletedDomainEvent : IDomainEvent
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
    /// Initializes a new instance of the SeasonCompletedDomainEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    public SeasonCompletedDomainEvent(Guid seasonId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
    }
}

/// <summary>
/// Event raised when a team is added to a season
/// </summary>
public class TeamAddedToSeasonDomainEvent : IDomainEvent
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
    /// Gets the ID of the team
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Initializes a new instance of the TeamAddedToSeasonDomainEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="teamId">The ID of the team</param>
    public TeamAddedToSeasonDomainEvent(Guid seasonId, Guid teamId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
        TeamId = teamId;
    }
}

/// <summary>
/// Event raised when a team is removed from a season
/// </summary>
public class TeamRemovedFromSeasonDomainEvent : IDomainEvent
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
    /// Gets the ID of the team
    /// </summary>
    public Guid TeamId { get; }

    /// <summary>
    /// Initializes a new instance of the TeamRemovedFromSeasonDomainEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="teamId">The ID of the team</param>
    public TeamRemovedFromSeasonDomainEvent(Guid seasonId, Guid teamId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
        TeamId = teamId;
    }
} 
