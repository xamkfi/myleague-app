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
    public required string FirstName { get; private set; }

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    public required string LastName { get; private set; }

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
    }

    protected Person(string firstName, string lastName, DateTime birthDate)
    {
        Id = Guid.NewGuid();
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        BirthDate = birthDate;
    }

    /// <summary>
    /// Updates the person's basic information
    /// </summary>
    /// <param name="firstName">The new first name</param>
    /// <param name="lastName">The new last name</param>
    public void UpdateBasicInfo(string firstName, string lastName)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
    }
} 
