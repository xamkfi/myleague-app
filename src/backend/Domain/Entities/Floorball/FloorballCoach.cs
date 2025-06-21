using Domain.Entities.Common;
using Domain.EventSourcing;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball coach in the system
/// </summary>
public class FloorballCoach : AggregateRoot
{
    /// <summary>
    /// Gets the ID of the person this coach profile belongs to (FK)
    /// </summary>
    public Guid PersonId { get; private set; }
    
    /// <summary>
    /// Gets whether the coach is currently active
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Gets the coaching experience in years
    /// </summary>
    public int YearsOfExperience { get; private set; }
    
    /// <summary>
    /// Gets the coach's certification level (if any)
    /// </summary>
    public string? CertificationLevel { get; private set; }
    
    /// <summary>
    /// Gets the coaching specialization (e.g., "Offense", "Defense", "Goalkeeper", "Head Coach")
    /// </summary>
    public string? Specialization { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballCoach()
    {
        Id = Guid.NewGuid();
        PersonId = Guid.Empty;
        IsActive = true;
        YearsOfExperience = 0;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballCoach class
    /// </summary>
    /// <param name="personId">The ID of the person this coach profile belongs to</param>
    /// <param name="yearsOfExperience">The coaching experience in years</param>
    /// <param name="certificationLevel">The coach's certification level (optional)</param>
    /// <param name="specialization">The coaching specialization (optional)</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballCoach(
        Guid personId,
        int yearsOfExperience = 0,
        string? certificationLevel = null,
        string? specialization = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));
            
        if (yearsOfExperience < 0)
            throw new ArgumentException("Years of experience cannot be negative.", nameof(yearsOfExperience));
            
        Id = Guid.NewGuid();
        PersonId = personId;
        IsActive = true;
        YearsOfExperience = yearsOfExperience;
        CertificationLevel = certificationLevel;
        Specialization = specialization;
    }

    /// <summary>
    /// Updates the coach's active status
    /// </summary>
    /// <param name="isActive">The new active status</param>
    public void UpdateActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }
    
    /// <summary>
    /// Updates the coach's experience
    /// </summary>
    /// <param name="yearsOfExperience">The new years of experience</param>
    public void UpdateExperience(int yearsOfExperience)
    {
        if (yearsOfExperience < 0)
            throw new ArgumentException("Years of experience cannot be negative.", nameof(yearsOfExperience));
            
        YearsOfExperience = yearsOfExperience;
    }
    
    /// <summary>
    /// Updates the coach's certification level
    /// </summary>
    /// <param name="certificationLevel">The new certification level</param>
    public void UpdateCertification(string? certificationLevel)
    {
        CertificationLevel = certificationLevel;
    }
    
    /// <summary>
    /// Updates the coach's specialization
    /// </summary>
    /// <param name="specialization">The new specialization</param>
    public void UpdateSpecialization(string? specialization)
    {
        Specialization = specialization;
    }
} 
