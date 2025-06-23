using Domain.Entities.Common;
using Domain.EventSourcing;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball team manager in the system
/// </summary>
public class FloorballTeamManager : AggregateRoot
{
    /// <summary>
    /// Gets the ID of the person this team manager profile belongs to (FK)
    /// </summary>
    public Guid PersonId { get; private set; }
    
    /// <summary>
    /// Gets the ID of the team this manager is responsible for (FK)
    /// </summary>
    public Guid TeamId { get; private set; }
    
    /// <summary>
    /// Gets whether the team manager is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTeamManager()
    {
        Id = Guid.NewGuid();
        PersonId = Guid.Empty;
        TeamId = Guid.Empty;
        IsActive = true;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballTeamManager class
    /// </summary>
    /// <param name="personId">The ID of the person this team manager profile belongs to</param>
    /// <param name="teamId">The ID of the team this manager is responsible for</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballTeamManager(
        Guid personId,
        Guid teamId)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));
            
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
            
        Id = Guid.NewGuid();
        PersonId = personId;
        TeamId = teamId;
        IsActive = true;
    }

    /// <summary>
    /// Updates the team manager's active status
    /// </summary>
    /// <param name="isActive">The new active status</param>
    public void UpdateActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }
    
    /// <summary>
    /// Updates the team that this manager is responsible for
    /// </summary>
    /// <param name="teamId">The new team ID</param>
    /// <exception cref="ArgumentException">Thrown when teamId is empty</exception>
    public void UpdateTeam(Guid teamId)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
            
        TeamId = teamId;
    }
} 
