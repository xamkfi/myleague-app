using System;

namespace Domain.ValueObjects.Common;

/// <summary>
/// Represents contact information as a value object.
///
/// All fields are optional. Player rosters imported from schedule sheets (PMT, ad-hoc tournaments,
/// etc.) often have no email at all, so the value object never throws on a missing/empty email —
/// it only enforces a basic format check when one is actually provided.
///
/// Callers that want to skip persisting a ContactInfo block entirely should pass `null` to
/// <see cref="Entities.Common.Person.UpdateContactInfo"/> rather than constructing an empty
/// instance. See <c>PersonMapper.ToContactInfo</c> for the "all-empty → null" convention.
/// </summary>
public class ContactInfo : IEquatable<ContactInfo>
{
    /// <summary>
    /// Gets the email address. Nullable — see class docs.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Gets the primary phone number
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Gets the alternative phone number
    /// </summary>
    public string? AlternativePhone { get; private set; }

    private ContactInfo()
    {
        Email = null;
    }

    /// <summary>
    /// Creates a new contact information object. Email may be null/empty; when non-empty it must
    /// pass a basic format check (`@` + `.`).
    /// </summary>
    public ContactInfo(string? email, string? phone = null, string? alternativePhone = null)
    {
        string? normalizedEmail = EmailAddress.NormalizeOptional(email);

        if (normalizedEmail != null
            && (!normalizedEmail.Contains('@', StringComparison.Ordinal)
                || !normalizedEmail.Contains('.', StringComparison.Ordinal)))
        {
            throw new ArgumentException("Email must be in a valid format", nameof(email));
        }

        Email = normalizedEmail;
        Phone = phone;
        AlternativePhone = alternativePhone;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ContactInfo);
    }

    public bool Equals(ContactInfo? other)
    {
        if (other is null)
            return false;

        return Email == other.Email &&
               Phone == other.Phone &&
               AlternativePhone == other.AlternativePhone;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Email, Phone, AlternativePhone);
    }

    public static bool operator ==(ContactInfo? left, ContactInfo? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(ContactInfo? left, ContactInfo? right)
    {
        return !(left == right);
    }
} 
