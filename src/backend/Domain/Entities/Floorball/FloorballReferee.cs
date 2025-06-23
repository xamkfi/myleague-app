using Domain.Entities;
using Domain.Entities.Common;
using Domain.EventSourcing;
using Domain.ValueObjects.Common;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball referee in the system
/// </summary>
public class FloorballReferee : AggregateRoot
{
    /// <summary>
    /// Gets the ID of the person this referee profile belongs to (FK)
    /// </summary>
    public Guid PersonId { get; private set; }
    
    /// <summary>
    /// Gets the person this referee profile belongs to
    /// </summary>
    public Person Person { get; private set; }
    
    /// <summary>
    /// Gets whether the referee is currently active
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Gets the date when the referee's license was issued
    /// </summary>
    public DateTime? LicenseIssueDate { get; private set; }
    
    /// <summary>
    /// Gets the date when the referee's license expires
    /// </summary>
    public DateTime? LicenseExpiryDate { get; private set; }
    
    /// <summary>
    /// Gets the number of floorball matches officiated by this referee
    /// </summary>
    public int MatchesOfficiated { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballReferee()
    {
        Id = Guid.NewGuid();
        Person = null!; // Explicitly mark as non-nullable to satisfy the compiler
        PersonId = Guid.Empty;
        IsActive = true;
        MatchesOfficiated = 0;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballReferee class
    /// </summary>
    /// <param name="personId">The ID of the person this referee profile belongs to</param>
    /// <param name="licenseIssueDate">The date when the license was issued</param>
    /// <param name="licenseExpiryDate">The date when the license expires</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballReferee(
        Guid personId,
        DateTime licenseIssueDate,
        DateTime licenseExpiryDate)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));
        
        if (licenseIssueDate > DateTime.UtcNow)
            throw new ArgumentException("License issue date cannot be in the future.", nameof(licenseIssueDate));
        
        if (licenseExpiryDate <= licenseIssueDate)
            throw new ArgumentException("License expiry date must be after the issue date.", nameof(licenseExpiryDate));
        
        Id = Guid.NewGuid();
        Person = null!; // Explicitly mark as non-nullable to satisfy the compiler
        PersonId = personId;
        IsActive = true;
        LicenseIssueDate = licenseIssueDate;
        LicenseExpiryDate = licenseExpiryDate;
        MatchesOfficiated = 0;
    }

    /// <summary>
    /// Updates the referee's active status
    /// </summary>
    /// <param name="isActive">The new active status</param>
    public void UpdateActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }

    /// <summary>
    /// Updates the referee's license expiry date
    /// </summary>
    /// <param name="newExpiryDate">The new expiry date</param>
    /// <exception cref="ArgumentException">Thrown when the new expiry date is invalid</exception>
    public void UpdateLicenseExpiry(DateTime newExpiryDate)
    {
        if (newExpiryDate <= DateTime.UtcNow)
            throw new ArgumentException("New expiry date must be in the future.", nameof(newExpiryDate));

        LicenseExpiryDate = newExpiryDate;
    }

    /// <summary>
    /// Records that the referee has officiated a match
    /// </summary>
    public void RecordMatchOfficiated()
    {
        MatchesOfficiated++;
    }
    
    /// <summary>
    /// Checks if the referee's license is valid as of a specific date
    /// </summary>
    /// <param name="checkDate">The date to check against</param>
    /// <returns>True if the license is valid on the specified date</returns>
    public bool HasValidLicense(DateTime checkDate)
    {
        return IsActive && LicenseExpiryDate.HasValue && checkDate <= LicenseExpiryDate;
    }
} 
