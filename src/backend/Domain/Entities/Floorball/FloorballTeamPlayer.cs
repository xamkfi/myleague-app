using Domain.Entities;
using Domain.Enums.Floorball;
using System;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a player's membership in a floorball team
/// </summary>
public class FloorballTeamPlayer : BaseEntity
{
    /// <summary>
    /// Gets the ID of the team
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the ID of the player
    /// </summary>
    public Guid PlayerId { get; private set; }

    /// <summary>
    /// Gets the player's position in the team
    /// </summary>
    public FloorballPosition Position { get; private set; }

    /// <summary>
    /// Gets whether the player is currently active in the team
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the jersey number of the player in this team
    /// </summary>
    public int? JerseyNumber { get; private set; }
    
    /// <summary>
    /// Gets the number of games played for this team
    /// </summary>
    public int GamesPlayed { get; private set; }
    
    /// <summary>
    /// Gets the number of goals scored for this team
    /// </summary>
    public int Goals { get; private set; }
    
    /// <summary>
    /// Gets the number of assists made for this team
    /// </summary>
    public int Assists { get; private set; }
    
    /// <summary>
    /// Gets the number of penalty minutes for this team
    /// </summary>
    public int PenaltyMinutes { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTeamPlayer()
    {
        IsActive = true;
        GamesPlayed = 0;
        Goals = 0;
        Assists = 0;
        PenaltyMinutes = 0;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballTeamPlayer class
    /// </summary>
    /// <param name="teamId">The ID of the team</param>
    /// <param name="playerId">The ID of the player</param>
    /// <param name="position">The player's position in the team</param>
    /// <param name="jerseyNumber">The player's jersey number in this team (optional)</param>
    public FloorballTeamPlayer(Guid teamId, Guid playerId, FloorballPosition position, int? jerseyNumber = null)
    {
        TeamId = teamId;
        PlayerId = playerId;
        Position = position;
        JerseyNumber = jerseyNumber;
        IsActive = true;
        GamesPlayed = 0;
        Goals = 0;
        Assists = 0;
        PenaltyMinutes = 0;
    }

    /// <summary>
    /// Updates the player's position in the team
    /// </summary>
    /// <param name="newPosition">The new position</param>
    public void UpdatePosition(FloorballPosition newPosition)
    {
        Position = newPosition;
    }

    /// <summary>
    /// Sets the player's active status in the team
    /// </summary>
    /// <param name="isActive">The new active status</param>
    public void SetActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }

    /// <summary>
    /// Updates the player's jersey number in this team
    /// </summary>
    /// <param name="jerseyNumber">The new jersey number</param>
    public void UpdateJerseyNumber(int? jerseyNumber)
    {
        JerseyNumber = jerseyNumber;
    }
    
    /// <summary>
    /// Records a game played by this player
    /// </summary>
    public void RecordGamePlayed()
    {
        GamesPlayed++;
    }
    
    /// <summary>
    /// Records a goal scored by this player
    /// </summary>
    public void RecordGoal()
    {
        Goals++;
    }
    
    /// <summary>
    /// Records an assist made by this player
    /// </summary>
    public void RecordAssist()
    {
        Assists++;
    }
    
    /// <summary>
    /// Records penalty minutes for this player
    /// </summary>
    /// <param name="minutes">The number of penalty minutes to add</param>
    public void RecordPenaltyMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentException("Penalty minutes cannot be negative.", nameof(minutes));

        PenaltyMinutes += minutes;
    }

    /// <summary>
    /// Removes a goal from this player's statistics
    /// </summary>
    public void RemoveGoal()
    {
        if (Goals > 0)
        {
            Goals--;
        }
    }

    /// <summary>
    /// Removes an assist from this player's statistics
    /// </summary>
    public void RemoveAssist()
    {
        if (Assists > 0)
        {
            Assists--;
        }
    }

    /// <summary>
    /// Removes penalty minutes from this player
    /// </summary>
    /// <param name="minutes">The number of penalty minutes to remove</param>
    public void RemovePenaltyMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentException("Penalty minutes to remove cannot be negative.", nameof(minutes));

        PenaltyMinutes = Math.Max(0, PenaltyMinutes - minutes);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object
    /// Based on identity (TeamId + PlayerId combination)
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not FloorballTeamPlayer other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // For entities, equality is based on identity (TeamId + PlayerId combination)
        return TeamId == other.TeamId && PlayerId == other.PlayerId;
    }

    /// <summary>
    /// Returns a hash code for the current object
    /// </summary>
    /// <returns>A hash code for the current object</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(TeamId, PlayerId);
    }

    /// <summary>
    /// Determines whether two FloorballTeamPlayer instances are equal
    /// </summary>
    /// <param name="left">The first instance to compare</param>
    /// <param name="right">The second instance to compare</param>
    /// <returns>true if the instances are equal; otherwise, false</returns>
    public static bool operator ==(FloorballTeamPlayer? left, FloorballTeamPlayer? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two FloorballTeamPlayer instances are not equal
    /// </summary>
    /// <param name="left">The first instance to compare</param>
    /// <param name="right">The second instance to compare</param>
    /// <returns>true if the instances are not equal; otherwise, false</returns>
    public static bool operator !=(FloorballTeamPlayer? left, FloorballTeamPlayer? right)
    {
        return !(left == right);
    }
} 