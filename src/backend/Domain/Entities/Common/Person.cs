using Domain.EventSourcing;
using Domain.ValueObjects.Common;
using Domain.DomainEvents.Common;

namespace Domain.Entities.Common;

/// <summary>
/// Represents a person in the system
/// </summary>
public class Person : AggregateRoot
{
    /// <summary>
    /// Gets the unique identifier of the person
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the first name of the person
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// Gets the birth date of the person
    /// </summary>
    public DateTime BirthDate { get; private set; }
    
    /// <summary>
    /// Gets the address of the person
    /// </summary>
    public Address? Address { get; private set; }
    
    /// <summary>
    /// Gets the contact information of the person
    /// </summary>
    public ContactInfo? ContactInfo { get; private set; }
    
    /// <summary>
    /// Gets the full name of the person (first + last)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected Person()
    {
        Id = Guid.NewGuid();
        FirstName = string.Empty;
        LastName = string.Empty;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Person"/> class with the specified details.
    /// </summary>
    /// <param name="firstName">The first name of the person.</param>
    /// <param name="lastName">The last name of the person.</param>
    /// <param name="birthDate">The birth date of the person.</param>
    /// <param name="address">The address of the person (optional).</param>
    /// <param name="contactInfo">The contact information of the person (optional).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="firstName"/> or <paramref name="lastName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="firstName"/> or <paramref name="lastName"/> is empty or whitespace, or if <paramref name="birthDate"/> is in the future.</exception>
    public Person(string firstName, string lastName, DateTime birthDate,
        Address? address = null, ContactInfo? contactInfo = null)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
        if (birthDate > DateTime.UtcNow)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Address = address;
        ContactInfo = contactInfo;

        AddDomainEvent(new PersonRegisteredEvent(Id, firstName, lastName, birthDate, address, contactInfo));
    }

    /// <summary>
    /// Updates the person's basic information
    /// </summary>
    /// <param name="firstName">The new first name</param>
    /// <param name="lastName">The new last name</param>
    public void UpdateBasicInfo(string firstName, string lastName)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
            
        FirstName = firstName;
        LastName = lastName;

        AddDomainEvent(new PersonInfoUpdatedEvent(Id, firstName, lastName));
    }

    public void UpdateBirthDate(DateTime birthDate)
    {
        if (birthDate > DateTime.UtcNow)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));

        BirthDate = birthDate;
    }

    /// <summary>
    /// Updates the person's address
    /// </summary>
    public void UpdateAddress(Address? address)
    {
        Address = address;
    }
    
    /// <summary>
    /// Updates the person's contact information
    /// </summary>
    public void UpdateContactInfo(ContactInfo? contactInfo)
    {
        ContactInfo = contactInfo;
    }
} 
