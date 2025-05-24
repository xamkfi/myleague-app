using Domain.DomainEvents;
using Domain.Enums.Floorball;

namespace Domain.DomainEvents.Floorball;

/// <summary>
/// Event raised when a floorball season's division is updated
/// </summary>
public class FloorballSeasonDivisionUpdatedEvent : IDomainEvent
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
    /// Gets the updated division of the season
    /// </summary>
    public FloorballDivision Division { get; }

    /// <summary>
    /// Initializes a new instance of the FloorballSeasonDivisionUpdatedEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="division">The updated division of the season</param>
    public FloorballSeasonDivisionUpdatedEvent(Guid seasonId, FloorballDivision division)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
        Division = division;
    }
} 