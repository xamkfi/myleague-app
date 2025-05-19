using Domain2.Enums.Floorball;
using Domain2.Entities;
using Domain2.Entities.Common;

namespace Domain2.Entities.Floorball;

/// <summary>
/// Represents a floorball player in the system
/// </summary>
public class FloorballPlayer : Person
{
    /// <summary>
    /// Gets whether the floorball player is currently active
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Gets the player's preferred floorball position
    /// </summary>
    public FloorballPosition PreferredPosition { get; private set; }
    
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
    private FloorballPlayer() : base()
    {
        IsActive = true;
        CareerGoals = 0;
        CareerAssists = 0;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballPlayer class
    /// </summary>
    /// <param name="firstName">The player's first name</param>
    /// <param name="lastName">The player's last name</param>
    /// <param name="birthDate">The player's birth date</param>
    /// <param name="preferredPosition">The player's preferred position</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballPlayer(
        string firstName, 
        string lastName, 
        DateTime birthDate,
        FloorballPosition preferredPosition = FloorballPosition.Forward)
        : base(firstName, lastName, birthDate)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
        if (birthDate > DateTime.UtcNow)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));

        IsActive = true;
        PreferredPosition = preferredPosition;
        CareerGoals = 0;
        CareerAssists = 0;
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
    /// Updates the player's preferred position
    /// </summary>
    /// <param name="position">The new preferred position</param>
    public void UpdatePreferredPosition(FloorballPosition position)
    {
        PreferredPosition = position;
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
} 