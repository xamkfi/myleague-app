using Domain.Enums.Floorball;
using Domain.Entities;
using Domain.Entities.Common;
using Domain.EventSourcing;
using Domain.ValueObjects.Floorball;
using Domain.DomainEvents.Floorball;
using Domain.ValueObjects.Common;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball player in the system
/// </summary>
public class FloorballPlayer : AggregateRoot
{
    /// <summary>
    /// Gets the ID of the person this player profile belongs to (FK)
    /// </summary>
    public Guid PersonId { get; private set; }
    
    /// <summary>
    /// Gets the person this player profile belongs to
    /// </summary>
    public Person Person { get; private set; }
    
    /// <summary>
    /// Gets whether the floorball player is currently active
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Gets the player's position information
    /// </summary>
    public Position Position { get; private set; }
    
    /// <summary>
    /// Gets the player's total career goals in floorball
    /// </summary>
    public int CareerGoals { get; private set; }
    
    /// <summary>
    /// Gets the player's total career assists in floorball
    /// </summary>
    public int CareerAssists { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballPlayer()
    {
        Id = Guid.NewGuid();
        PersonId = Guid.Empty;
        Person = null!; // Explicitly mark as non-nullable to satisfy the compiler
        IsActive = true;
        Position = new Position(FloorballPosition.None);
        CareerGoals = 0;
        CareerAssists = 0;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballPlayer class
    /// </summary>
    /// <param name="personId">The ID of the person this player profile belongs to</param>
    /// <param name="position">The player's position information</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballPlayer(Guid personId, Position position)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));
        
        ArgumentNullException.ThrowIfNull(position);
            
        Id = Guid.NewGuid();
        Person = null!; // Explicitly mark as non-nullable to satisfy the compiler
        PersonId = personId;
        IsActive = true;
        Position = position;
        CareerGoals = 0;
        CareerAssists = 0;
        
        AddDomainEvent(new FloorballPlayerRegisteredEvent(Id, personId, position));
    }

    /// <summary>
    /// Updates the player's active status
    /// </summary>
    /// <param name="isActive">The new active status</param>
    public void UpdateActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }
    
    /// <summary>
    /// Updates the player's position
    /// </summary>
    /// <param name="position">The new position</param>
    public void UpdatePosition(Position position)
    {
        ArgumentNullException.ThrowIfNull(position);
        Position = position;
        
        AddDomainEvent(new FloorballPlayerPositionUpdatedEvent(Id, position));
    }
    
    /// <summary>
    /// Records a goal for the player
    /// </summary>
    public void RecordGoal()
    {
        CareerGoals++;
        
        AddDomainEvent(new FloorballPlayerStatUpdatedEvent(Id, CareerGoals, CareerAssists, StatUpdateType.Goal));
    }
    
    /// <summary>
    /// Records an assist for the player
    /// </summary>
    public void RecordAssist()
    {
        CareerAssists++;
        
        AddDomainEvent(new FloorballPlayerStatUpdatedEvent(Id, CareerGoals, CareerAssists, StatUpdateType.Assist));
    }
    
    /// <summary>
    /// Sets the person for this player (used when loading navigation properties)
    /// </summary>
    /// <param name="person">The person to associate with this player</param>
    public void SetPerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);
        Person = person;
    }
} 
