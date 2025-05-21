using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a club is registered
/// </summary>
public class ClubRegisteredEvent : CommonDomainEvent
{
    /// <summary>
    /// Gets the ID of the club
    /// </summary>
    public Guid ClubId { get; }

    /// <summary>
    /// Gets the name of the club
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the city where the club is based
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the country where the club is based
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Gets the founding date of the club
    /// </summary>
    public DateTime FoundingDate { get; }

    /// <summary>
    /// Initializes a new instance of the ClubRegisteredEvent class
    /// </summary>
    /// <param name="clubId">The ID of the club</param>
    /// <param name="name">The name of the club</param>
    /// <param name="city">The city where the club is based</param>
    /// <param name="country">The country where the club is based</param>
    /// <param name="foundingDate">The founding date of the club</param>
    public ClubRegisteredEvent(Guid clubId, string name, string city, string country, DateTime foundingDate)
    {
        ClubId = clubId;
        Name = name;
        City = city;
        Country = country;
        FoundingDate = foundingDate;
    }
} 