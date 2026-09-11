using Domain.Entities.Common;

namespace Domain.Entities.Football.Teams;

/// <summary>
/// A football referee profile attached to a Person.
/// </summary>
public class FootballReferee : BaseEntity
{
    public Guid PersonId { get; private set; }
    public Person Person { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LicenseIssueDate { get; private set; }
    public DateTime? LicenseExpiryDate { get; private set; }
    public int MatchesOfficiated { get; private set; }

    private FootballReferee()
    {
        Person = null!;
        PersonId = Guid.Empty;
        IsActive = true;
    }

    public FootballReferee(Guid personId, DateTime licenseIssueDate, DateTime licenseExpiryDate)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person ID cannot be empty.", nameof(personId));
        if (licenseIssueDate > DateTime.UtcNow)
            throw new ArgumentException("License issue date cannot be in the future.", nameof(licenseIssueDate));
        if (licenseExpiryDate <= licenseIssueDate)
            throw new ArgumentException("License expiry date must be after the issue date.", nameof(licenseExpiryDate));

        Person = null!;
        PersonId = personId;
        IsActive = true;
        LicenseIssueDate = licenseIssueDate;
        LicenseExpiryDate = licenseExpiryDate;
    }

    public void UpdateActiveStatus(bool isActive) => IsActive = isActive;

    public void UpdateLicenseExpiry(DateTime newExpiryDate)
    {
        if (newExpiryDate <= DateTime.UtcNow)
            throw new ArgumentException("New expiry date must be in the future.", nameof(newExpiryDate));
        LicenseExpiryDate = newExpiryDate;
    }

    public void RecordMatchOfficiated() => MatchesOfficiated++;

    public void UpdateMatchesOfficiated(int matchesOfficiated)
    {
        if (matchesOfficiated < 0)
            throw new ArgumentException("Matches officiated cannot be negative.", nameof(matchesOfficiated));
        MatchesOfficiated = matchesOfficiated;
    }

    public bool HasValidLicense(DateTime checkDate) =>
        IsActive && LicenseExpiryDate.HasValue && checkDate <= LicenseExpiryDate;

    public void SetPerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);
        Person = person;
    }
}
