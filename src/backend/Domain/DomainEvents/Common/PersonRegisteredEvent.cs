using Domain.DomainEvents;
using Domain.ValueObjects.Common;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a person is registered
/// </summary>
public class PersonRegisteredEvent : IDomainEvent
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
    /// Gets the first name of the person
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Gets the birth date of the person
    /// </summary>
    public DateTime BirthDate { get; }

    /// <summary>
    /// Gets the address of the person
    /// </summary>
    public Address? Address { get; }

    /// <summary>
    /// Gets the contact information of the person
    /// </summary>
    public ContactInfo? ContactInfo { get; }

    /// <summary>
    /// Initializes a new instance of the PersonRegisteredEvent class
    /// </summary>
    /// <param name="personId">The ID of the person</param>
    /// <param name="firstName">The first name of the person</param>
    /// <param name="lastName">The last name of the person</param>
    /// <param name="birthDate">The birth date of the person</param>
    /// <param name="address">The address of the person</param>
    /// <param name="contactInfo">The contact information of the person</param>
    public PersonRegisteredEvent(
        Guid personId, 
        string firstName, 
        string lastName, 
        DateTime birthDate,
        Address? address = null,
        ContactInfo? contactInfo = null)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        PersonId = personId;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Address = address;
        ContactInfo = contactInfo;
    }
} 