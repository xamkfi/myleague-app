using Domain.EventSourcing;
using Domain.DomainEvents.Common;

namespace Domain.Entities.Common;

/// <summary>
/// Represents a division in sports leagues
/// </summary>
public class Division : AggregateRoot
{
    /// <summary>
    /// Gets the unique identifier of the division
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the division
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of the division
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the level of the division (used for ordering, lower numbers = higher level)
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// Gets the sport type this division belongs to
    /// </summary>
    public string SportType { get; private set; }

    /// <summary>
    /// Gets whether this division is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the creation date of the division
    /// </summary>
    public DateTime CreatedDate { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private Division()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        Description = string.Empty;
        SportType = string.Empty;
        IsActive = true;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the Division class
    /// </summary>
    /// <param name="name">The name of the division</param>
    /// <param name="description">The description of the division</param>
    /// <param name="level">The level of the division (lower numbers = higher level)</param>
    /// <param name="sportType">The sport type this division belongs to</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public Division(string name, string description, int level, string sportType)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(sportType);
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Division name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(sportType))
            throw new ArgumentException("Sport type cannot be null or empty.", nameof(sportType));
        if (level < 0)
            throw new ArgumentException("Division level cannot be negative.", nameof(level));

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Level = level;
        SportType = sportType;
        IsActive = true;
        CreatedDate = DateTime.UtcNow;

        AddDomainEvent(new DivisionCreatedEvent(Id, name, description, level, sportType));
    }

    /// <summary>
    /// Updates the division's details
    /// </summary>
    /// <param name="name">The new name</param>
    /// <param name="description">The new description</param>
    /// <param name="level">The new level</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public void UpdateDetails(string name, string description, int level)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Division name cannot be null or empty.", nameof(name));
        if (level < 0)
            throw new ArgumentException("Division level cannot be negative.", nameof(level));

        Name = name;
        Description = description;
        Level = level;

        AddDomainEvent(new DivisionUpdatedEvent(Id, name, description, level));
    }

    /// <summary>
    /// Activates the division
    /// </summary>
    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            AddDomainEvent(new DivisionActivatedEvent(Id));
        }
    }

    /// <summary>
    /// Deactivates the division
    /// </summary>
    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            AddDomainEvent(new DivisionDeactivatedEvent(Id));
        }
    }
} 