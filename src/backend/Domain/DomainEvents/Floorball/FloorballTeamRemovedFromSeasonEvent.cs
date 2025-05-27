using Domain.DomainEvents;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a team is removed from a floorball season
/// </summary>
public class FloorballTeamRemovedFromSeasonEvent : IDomainEvent
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
    /// Initializes a new instance of the FloorballTeamRemovedFromSeasonEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="teamId">The ID of the team</param>
    public FloorballTeamRemovedFromSeasonEvent(Guid seasonId, Guid teamId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
        TeamId = teamId;
    }
} 