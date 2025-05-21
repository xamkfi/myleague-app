using Domain.Entities;
using Domain.Entities.Common;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents a floorball referee in the system
/// </summary>
public class FloorballReferee : Person
{
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
    private FloorballReferee() : base()
    {
        IsActive = true;
        MatchesOfficiated = 0;
    }

    /// <summary>
    /// Initializes a new instance of the FloorballReferee class
    /// </summary>
    /// <param name="firstName">The referee's first name</param>
    /// <param name="lastName">The referee's last name</param>
    /// <param name="birthDate">The referee's birth date</param>
    /// <param name="licenseIssueDate">The date when the license was issued</param>
    /// <param name="licenseExpiryDate">The date when the license expires</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public FloorballReferee(
        string firstName,
        string lastName,
        DateTime birthDate,
        DateTime licenseIssueDate,
        DateTime licenseExpiryDate)
        : base(firstName, lastName, birthDate)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
        if (birthDate > DateTime.UtcNow)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));
        if (licenseIssueDate > DateTime.UtcNow)
            throw new ArgumentException("License issue date cannot be in the future.", nameof(licenseIssueDate));
        if (licenseExpiryDate <= licenseIssueDate)
            throw new ArgumentException("License expiry date must be after the issue date.", nameof(licenseExpiryDate));
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
        return IsActive && checkDate <= LicenseExpiryDate;
    }
} 
