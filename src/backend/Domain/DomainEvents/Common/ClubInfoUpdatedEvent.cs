using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a club's information is updated
/// </summary>
public class ClubInfoUpdatedEvent : IDomainEvent
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
    /// Gets the ID of the club
    /// </summary>
    public Guid ClubId { get; }

    /// <summary>
    /// Gets the updated name of the club
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the updated city where the club is based
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the updated country where the club is based
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Initializes a new instance of the ClubInfoUpdatedEvent class
    /// </summary>
    /// <param name="clubId">The ID of the club</param>
    /// <param name="name">The updated name of the club</param>
    /// <param name="city">The updated city where the club is based</param>
    /// <param name="country">The updated country where the club is based</param>
    public ClubInfoUpdatedEvent(Guid clubId, string name, string city, string country)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ClubId = clubId;
        Name = name;
        City = city;
        Country = country;
    }
} 