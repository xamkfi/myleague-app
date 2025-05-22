using Domain.DomainEvents;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a hockey season's date range is updated
/// </summary>
public class HockeySeasonDateRangeUpdatedEvent : IDomainEvent
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
    /// Gets the updated start date of the season
    /// </summary>
    public DateTime StartDate { get; }

    /// <summary>
    /// Gets the updated end date of the season
    /// </summary>
    public DateTime EndDate { get; }

    /// <summary>
    /// Initializes a new instance of the HockeySeasonDateRangeUpdatedEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="startDate">The updated start date of the season</param>
    /// <param name="endDate">The updated end date of the season</param>
    public HockeySeasonDateRangeUpdatedEvent(Guid seasonId, DateTime startDate, DateTime endDate)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
        StartDate = startDate;
        EndDate = endDate;
    }
} 