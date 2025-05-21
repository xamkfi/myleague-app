using Domain.EventSourcing;
using Domain.ValueObjects.Common;

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
