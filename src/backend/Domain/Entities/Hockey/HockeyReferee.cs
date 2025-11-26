using Domain.Entities;
using Domain.Entities.Common;

namespace Domain.Entities.Hockey;

/// <summary>
/// Represents a Hockey referee in the system
/// </summary>
public class HockeyReferee : Person
{
    /// <summary>
    /// Gets whether the referee is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the date when the referee's license was issued
    /// </summary>
    public DateTime LicenseIssueDate { get; private set; }

    /// <summary>
    /// Gets the date when the referee's license expires
    /// </summary>
    public DateTime LicenseExpiryDate { get; private set; }

    /// <summary>
    /// Gets the number of Hockey matches officiated by this referee
    /// </summary>
    public int MatchesOfficiated { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private HockeyReferee() : base()
    {
        IsActive = true;
        MatchesOfficiated = 0;
    }

    /// <summary>
    /// Initializes a new instance of the HockeyReferee class
    /// </summary>
    /// <param name="firstName">The referee's first name</param>
    /// <param name="lastName">The referee's last name</param>
    /// <param name="birthDate">The referee's birth date (optional)</param>
    /// <param name="licenseIssueDate">The date when the license was issued</param>
    /// <param name="licenseExpiryDate">The date when the license expires</param>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    public HockeyReferee(
        string firstName,
        string lastName,
        DateTime licenseIssueDate,
        DateTime licenseExpiryDate,
        DateTime? birthDate = null)
        : base(firstName, lastName, birthDate)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
        if (birthDate.HasValue && birthDate.Value > DateTime.UtcNow)
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
