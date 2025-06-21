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
    /// Gets whether the team manager is currently active
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Gets the team manager's primary responsibility area
    /// </summary>
    public string? PrimaryResponsibility { get; private set; }
    
    /// <summary>
    /// Gets the years of experience as a team manager
    /// </summary>
    public int YearsOfExperience { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTeamManager()
    {
        Id = Guid.NewGuid();
        PersonId = Guid.Empty;
        IsActive = true;
        YearsOfExperience = 0;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballTeamManager class
    /// </summary>
    /// <param name="personId">The ID of the person this team manager profile belongs to</param>
    /// <param name="primaryResponsibility">The primary responsibility area (optional)</param>
    /// <param name="yearsOfExperience">The years of experience (optional)</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballTeamManager(
        Guid personId,
        string? primaryResponsibility = null,
        int yearsOfExperience = 0)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));
            
        if (yearsOfExperience < 0)
            throw new ArgumentException("Years of experience cannot be negative.", nameof(yearsOfExperience));
            
        Id = Guid.NewGuid();
        PersonId = personId;
        IsActive = true;
        PrimaryResponsibility = primaryResponsibility;
        YearsOfExperience = yearsOfExperience;
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
    /// Updates the team manager's primary responsibility
    /// </summary>
    /// <param name="primaryResponsibility">The new primary responsibility</param>
    public void UpdatePrimaryResponsibility(string? primaryResponsibility)
    {
        PrimaryResponsibility = primaryResponsibility;
    }
    
    /// <summary>
    /// Updates the team manager's experience
    /// </summary>
    /// <param name="yearsOfExperience">The new years of experience</param>
    public void UpdateExperience(int yearsOfExperience)
    {
        if (yearsOfExperience < 0)
            throw new ArgumentException("Years of experience cannot be negative.", nameof(yearsOfExperience));
            
        YearsOfExperience = yearsOfExperience;
    }
} 
