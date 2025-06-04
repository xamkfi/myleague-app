using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a person's information is updated
/// </summary>
public class PersonInfoUpdatedEvent : IDomainEvent
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
    /// Gets the ID of the person
    /// </summary>
    public Guid PersonId { get; }

    /// <summary>
    /// Gets the updated first name of the person
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Gets the updated last name of the person
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Initializes a new instance of the PersonInfoUpdatedEvent class
    /// </summary>
    /// <param name="personId">The ID of the person</param>
    /// <param name="firstName">The updated first name of the person</param>
    /// <param name="lastName">The updated last name of the person</param>
    public PersonInfoUpdatedEvent(Guid personId, string firstName, string lastName)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        PersonId = personId;
        FirstName = firstName;
        LastName = lastName;
    }
} 