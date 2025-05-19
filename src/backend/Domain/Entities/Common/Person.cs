using Domain.EventSourcing;

namespace Domain.Entities.Common;

/// <summary>
/// Represents a person in the system, serving as a base class for players and referees
/// </summary>
public abstract class Person : AggregateRoot
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
    /// Protected constructor for EF Core
    /// </summary>
    protected Person()
    {
        Id = Guid.NewGuid();
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    protected Person(string firstName, string lastName, DateTime birthDate)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
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
        FirstName = firstName;
        LastName = lastName;
    }
} 
