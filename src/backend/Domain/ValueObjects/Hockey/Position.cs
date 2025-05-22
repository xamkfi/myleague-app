using System;
using Domain.Enums.Hockey;

namespace Domain.ValueObjects.Hockey;

/// <summary>
/// Represents a hockey player position as a value object
/// </summary>
public class Position : IEquatable<Position>
{
    /// <summary>
    /// Gets the primary position
    /// </summary>
    public HockeyPosition PrimaryPosition { get; }
    
    /// <summary>
    /// Gets the secondary position (optional)
    /// </summary>
    public HockeyPosition? SecondaryPosition { get; }
    
    /// <summary>
    /// Gets if the player can play as a goalkeeper
    /// </summary>
    public bool CanPlayAsGoalkeeper { get; }
    
    /// <summary>
    /// Creates a new position value object
    /// </summary>
    public Position(
        HockeyPosition primaryPosition, 
        HockeyPosition? secondaryPosition = null, 
        bool canPlayAsGoalkeeper = false)
    {
        if (primaryPosition != HockeyPosition.None && primaryPosition == secondaryPosition)
            throw new ArgumentException("Primary and secondary positions cannot be the same", 
                nameof(secondaryPosition));
        
        // If primary position is goalkeeper, they can play as goalkeeper
        if (primaryPosition == HockeyPosition.Goalkeeper)
            canPlayAsGoalkeeper = true;
        
        PrimaryPosition = primaryPosition;
        SecondaryPosition = secondaryPosition;
        CanPlayAsGoalkeeper = canPlayAsGoalkeeper;
    }
    
    /// <summary>
    /// Gets if the player can play in a specific position
    /// </summary>
    public bool CanPlayInPosition(HockeyPosition position)
    {
        if (position == HockeyPosition.None)
            return false;
            
        if (position == HockeyPosition.Goalkeeper)
            return CanPlayAsGoalkeeper;
            
        return position == PrimaryPosition || position == SecondaryPosition;
    }
    
    /// <summary>
    /// Creates a new position with updated primary position
    /// </summary>
    public Position WithPrimaryPosition(HockeyPosition newPrimaryPosition)
    {
        return new Position(newPrimaryPosition, SecondaryPosition, CanPlayAsGoalkeeper);
    }
    
    /// <summary>
    /// Creates a new position with updated secondary position
    /// </summary>
    public Position WithSecondaryPosition(HockeyPosition? newSecondaryPosition)
    {
        return new Position(PrimaryPosition, newSecondaryPosition, CanPlayAsGoalkeeper);
    }
    
    /// <summary>
    /// Creates a new position with updated goalkeeper capability
    /// </summary>
    public Position WithGoalkeeperCapability(bool canPlayAsGoalkeeper)
    {
        return new Position(PrimaryPosition, SecondaryPosition, canPlayAsGoalkeeper);
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as Position);
    }

    public bool Equals(Position? other)
    {
        if (other is null)
            return false;

        return PrimaryPosition == other.PrimaryPosition && 
               SecondaryPosition == other.SecondaryPosition &&
               CanPlayAsGoalkeeper == other.CanPlayAsGoalkeeper;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PrimaryPosition, SecondaryPosition, CanPlayAsGoalkeeper);
    }

    public static bool operator ==(Position? left, Position? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(Position? left, Position? right) => !(left == right);
} 