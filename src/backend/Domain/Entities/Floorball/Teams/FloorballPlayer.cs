using Domain.Enums.Floorball;
using Domain.Entities;
using Domain.Entities.Common;
using Domain.ValueObjects.Floorball;
using Domain.ValueObjects.Common;

using Domain.Entities.Floorball.Competitions;
using Domain.Entities.Floorball.Matches;
using Domain.Entities.Floorball.Matches.Events;
using Domain.Entities.Floorball.Officials;
using Domain.Entities.Floorball.Statistics;

namespace Domain.Entities.Floorball.Teams;

/// <summary>
/// Represents a floorball player in the system
/// </summary>
public class FloorballPlayer : BaseEntity
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
    /// Gets whether this profile is the team's reusable loan goalkeeper placeholder.
    /// </summary>
    public bool IsLoanGoalkeeper { get; private set; }

    /// <summary>
    /// Gets whether this profile is a reusable loan-player placeholder.
    /// </summary>
    public bool IsLoanPlayer { get; private set; }

    /// <summary>
    /// Gets the 1-based sequence used in the name Lainapelaaja #N. Zero when this is not a loan player.
    /// </summary>
    public int LoanPlayerNumber { get; private set; }

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
        IsLoanGoalkeeper = false;
        IsLoanPlayer = false;
        LoanPlayerNumber = 0;
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
        IsLoanGoalkeeper = false;
        IsLoanPlayer = false;
        LoanPlayerNumber = 0;
    }

    /// <summary>
    /// Marks this profile as the team's reusable loan goalkeeper.
    /// </summary>
    public void MarkAsLoanGoalkeeper()
    {
        IsLoanGoalkeeper = true;
    }

    /// <summary>
    /// Marks this profile as a reusable loan player with a stable sequence number.
    /// </summary>
    /// <param name="loanPlayerNumber">1-based sequence shown as Lainapelaaja #N</param>
    public void MarkAsLoanPlayer(int loanPlayerNumber)
    {
        if (loanPlayerNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(loanPlayerNumber), "Loan player number must be at least 1.");

        IsLoanPlayer = true;
        LoanPlayerNumber = loanPlayerNumber;
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
        
    }
    
    /// <summary>
    /// Records a goal for the player
    /// </summary>
    public void RecordGoal()
    {
        CareerGoals++;
        
    }
    
    /// <summary>
    /// Records an assist for the player
    /// </summary>
    public void RecordAssist()
    {
        CareerAssists++;
    }

    /// <summary>
    /// Removes a goal from the player's career statistics
    /// </summary>
    public void RemoveGoal()
    {
        if (CareerGoals > 0)
        {
            CareerGoals--;

        }
    }

    /// <summary>
    /// Removes an assist from the player's career statistics
    /// </summary>
    public void RemoveAssist()
    {
        if (CareerAssists > 0)
        {
            CareerAssists--;

        }
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
