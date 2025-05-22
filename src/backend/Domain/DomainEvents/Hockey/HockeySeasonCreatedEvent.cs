using Domain.DomainEvents;
using Domain.Enums.Hockey;

namespace Domain.DomainEvents.Hockey;

/// <summary>
/// Event raised when a hockey season is created
/// </summary>
public class HockeySeasonCreatedEvent : IDomainEvent
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
    /// Gets the name of the season
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the division of the season
    /// </summary>
    public HockeyDivision Division { get; }

    /// <summary>
    /// Gets the start date of the season
    /// </summary>
    public DateTime StartDate { get; }

    /// <summary>
    /// Gets the end date of the season
    /// </summary>
    public DateTime EndDate { get; }

    /// <summary>
    /// Initializes a new instance of the HockeySeasonCreatedEvent class
    /// </summary>
    /// <param name="seasonId">The ID of the season</param>
    /// <param name="name">The name of the season</param>
    /// <param name="division">The division of the season</param>
    /// <param name="startDate">The start date of the season</param>
    /// <param name="endDate">The end date of the season</param>
    public HockeySeasonCreatedEvent(
        Guid seasonId, 
        string name, 
        HockeyDivision division, 
        DateTime startDate, 
        DateTime endDate)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        SeasonId = seasonId;
        Name = name;
        Division = division;
        StartDate = startDate;
        EndDate = endDate;
    }
} 