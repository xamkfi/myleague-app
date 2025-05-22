using Domain.DomainEvents;
using Domain.Enums.Hockey;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a hockey season's division is updated
/// </summary>
public class HockeySeasonDivisionUpdatedEvent : IDomainEvent
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
    public HockeyDivision Division { get; }

    /// <summary>
    /// Initializes a new instance of the HockeySeasonDivisionUpdatedEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="division">The updated division of the season</param>
    public HockeySeasonDivisionUpdatedEvent(Guid seasonId, HockeyDivision division)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
        Division = division;
    }
} 