using Domain.Entities.Common;
using Domain.Enums.Hockey.Teams;

namespace Domain.Entities.Hockey.Teams;

/// <summary>
/// Official profile linked to a <see cref="Person"/> via <see cref="PersonId"/>,
/// using the same cross-context pattern as <c>FloorballReferee</c>.
/// </summary>
public class HockeyOfficial : BaseEntity
{
    public Guid PersonId { get; private set; }
    public Person Person { get; private set; } = null!;
    public string? OfficialNumber { get; private set; }
    public HockeyOfficialRole OfficialRole { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LicenseIssueDate { get; private set; }
    public DateTime? LicenseExpiryDate { get; private set; }
    public int MatchesOfficiated { get; private set; }

    private HockeyOfficial() { }

    public HockeyOfficial(
        Guid personId,
        HockeyOfficialRole officialRole,
        string? officialNumber = null,
        DateTime? licenseIssueDate = null,
        DateTime? licenseExpiryDate = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Person id cannot be empty.", nameof(personId));
        if (licenseIssueDate is not null && licenseExpiryDate is not null && licenseExpiryDate <= licenseIssueDate)
            throw new ArgumentException("License expiry date must be after the issue date.", nameof(licenseExpiryDate));

        PersonId = personId;
        OfficialRole = officialRole;
        OfficialNumber = officialNumber;
        LicenseIssueDate = licenseIssueDate;
        LicenseExpiryDate = licenseExpiryDate;
        IsActive = true;
    }

    public void UpdateActiveStatus(bool isActive) => IsActive = isActive;

    public void UpdateOfficialRole(HockeyOfficialRole officialRole) => OfficialRole = officialRole;

    public void UpdateOfficialNumber(string? officialNumber) => OfficialNumber = officialNumber;

    public void UpdateLicenseExpiry(DateTime? licenseExpiryDate) => LicenseExpiryDate = licenseExpiryDate;

    public void RecordMatchOfficiated() => MatchesOfficiated++;

    public bool HasValidLicense(DateTime checkDate) =>
        IsActive && LicenseExpiryDate.HasValue && checkDate <= LicenseExpiryDate;

    public void SetPerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);
        Person = person;
    }
}
