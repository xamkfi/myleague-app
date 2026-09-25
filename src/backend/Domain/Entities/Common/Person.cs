using Domain.ValueObjects.Common;
using Domain.Enums.Common;

namespace Domain.Entities.Common;

/// <summary>
/// Represents a person in the system
/// </summary>
public class Person : BaseEntity
{
    /// <summary>
    /// Gets the first name of the person
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// Gets the role of the person
    /// </summary>
    public PersonRole role { get; private set; }

    /// <summary>
    /// Gets the birth date of the person
    /// </summary>
    public DateTime? BirthDate { get; private set; }
    
    /// <summary>
    /// Gets the address of the person
    /// </summary>
    public Address? Address { get; private set; }
    
    /// <summary>
    /// Gets the contact information of the person
    /// </summary>
    public ContactInfo? ContactInfo { get; private set; }
    
    /// <summary>
    /// Gets the full name of the person (first + last)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    public bool IsRegistered { get; private set; }

    /// <summary>
    /// Gets whether processing of this person's data is restricted.
    /// </summary>
    public bool IsProcessingRestricted { get; private set; }

    /// <summary>
    /// Gets whether the person has objected to processing based on legitimate interest.
    /// </summary>
    public bool HasObjectedToLegitimateInterest { get; private set; }

    /// <summary>
    /// Gets the UTC time identity data was anonymized, or null while the person is identifiable.
    /// </summary>
    public DateTime? AnonymizedAt { get; private set; }

    /// <summary>
    /// Gets whether identity data has been removed.
    /// </summary>
    public bool IsAnonymized => AnonymizedAt.HasValue;

    /// <summary>
    /// First name stored after erasure.
    /// </summary>
    public const string AnonymizedFirstName = "Removed";

    /// <summary>
    /// Last name stored after erasure.
    /// </summary>
    public const string AnonymizedLastName = "Person";

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected Person()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Person"/> class with the specified details.
    /// </summary>
    /// <param name="firstName">The first name of the person.</param>
    /// <param name="lastName">The last name of the person.</param>
    /// <param name="birthDate">The birth date of the person (optional).</param>
    /// <param name="role">The role of the person (defaults to User).</param>
    /// <param name="address">The address of the person (optional).</param>
    /// <param name="contactInfo">The contact information of the person (optional).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="firstName"/> or <paramref name="lastName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="firstName"/> or <paramref name="lastName"/> is empty or whitespace, or if <paramref name="birthDate"/> is in the future.</exception>
    public Person(string firstName, string lastName, DateTime? birthDate = null,
        PersonRole role = PersonRole.User, Address? address = null, ContactInfo? contactInfo = null)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
        if (birthDate.HasValue && birthDate.Value > DateTime.UtcNow)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));

        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        this.role = role;
        Address = address;
        ContactInfo = contactInfo;
    }

    /// <summary>
    /// Updates the person's basic information
    /// </summary>
    /// <param name="firstName">The new first name</param>
    /// <param name="lastName">The new last name</param>
    public void UpdateBasicInfo(string firstName, string lastName)
    {
        EnsureIdentityCanChange();
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
            
        FirstName = firstName;
        LastName = lastName;

    }

    /// <summary>
    /// Updates the person's birthdate
    /// </summary>
    public void UpdateBirthDate(DateTime? birthDate)
    {
        EnsureIdentityCanChange();
        if (birthDate.HasValue && birthDate.Value > DateTime.UtcNow)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));

        BirthDate = birthDate;
    }

    /// <summary>
    /// Updates the person's address
    /// </summary>
    public void UpdateAddress(Address? address)
    {
        EnsureIdentityCanChange();
        Address = address;
    }
    
    /// <summary>
    /// Updates the person's contact information
    /// </summary>
    public void UpdateContactInfo(ContactInfo? contactInfo)
    {
        EnsureIdentityCanChange();
        ContactInfo = contactInfo;
    }

    /// <summary>
    /// Corrects identity data the person supplied. Allowed while processing is restricted.
    /// </summary>
    public void Rectify(string firstName, string lastName, DateTime? birthDate, Address? address, ContactInfo? contactInfo)
    {
        UpdateBasicInfo(firstName, lastName);
        UpdateBirthDate(birthDate);
        UpdateAddress(address);
        UpdateContactInfo(contactInfo);
    }

    /// <summary>
    /// Marks processing as restricted. Login is suspended separately on the user account.
    /// </summary>
    public void RestrictProcessing()
    {
        IsProcessingRestricted = true;
    }

    /// <summary>
    /// Lifts a processing restriction. Anonymized identity data stays restricted.
    /// </summary>
    public void LiftProcessingRestriction()
    {
        if (IsAnonymized)
        {
            throw new InvalidOperationException("Anonymized personal data stays restricted.");
        }

        IsProcessingRestricted = false;
    }

    /// <summary>
    /// Records an objection to legitimate-interest processing and restricts that processing.
    /// </summary>
    public void ObjectToLegitimateInterest()
    {
        HasObjectedToLegitimateInterest = true;
        IsProcessingRestricted = true;
    }

    /// <summary>
    /// Withdraws an objection. A separate restriction stays in place until it is lifted.
    /// </summary>
    public void WithdrawLegitimateInterestObjection()
    {
        if (IsAnonymized)
        {
            throw new InvalidOperationException("Anonymized personal data cannot resume legitimate-interest processing.");
        }

        HasObjectedToLegitimateInterest = false;
    }

    /// <summary>
    /// Removes identity data. League rows that still reference this person keep the anonymized name.
    /// </summary>
    public void Anonymize(DateTime anonymizedAtUtc)
    {
        if (anonymizedAtUtc == default)
        {
            throw new ArgumentException("Anonymized time is required.", nameof(anonymizedAtUtc));
        }

        DateTime utc = anonymizedAtUtc.Kind == DateTimeKind.Local
            ? anonymizedAtUtc.ToUniversalTime()
            : DateTime.SpecifyKind(anonymizedAtUtc, DateTimeKind.Utc);

        FirstName = AnonymizedFirstName;
        LastName = AnonymizedLastName;
        BirthDate = null;
        Address = null;
        ContactInfo = null;
        IsProcessingRestricted = true;
        if (AnonymizedAt is null)
        {
            AnonymizedAt = utc;
        }
    }

    private void EnsureIdentityCanChange()
    {
        if (IsAnonymized)
        {
            throw new InvalidOperationException("Anonymized personal data cannot be changed.");
        }
    }

    public void UpdateIsRegistered(bool isRegistered)
    {
        IsRegistered = isRegistered;
    }

    /// <summary>
    /// Updates the person's role
    /// </summary>
    public void UpdateRole(PersonRole role)
    {
        this.role = role;
    }
} 
