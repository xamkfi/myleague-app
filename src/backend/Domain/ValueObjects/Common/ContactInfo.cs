using System;

namespace Domain.ValueObjects.Common;

/// <summary>
/// Represents contact information as a value object
/// </summary>
public class ContactInfo : IEquatable<ContactInfo>
{
    /// <summary>
    /// Gets the email address
    /// </summary>
    public string Email { get; private set; }
    
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
        Email = string.Empty;
    }

    /// <summary>
    /// Creates a new contact information object
    /// </summary>
    public ContactInfo(string email, string? phone = null, string? alternativePhone = null)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        // Simple email format validation
        // Updated email validation with StringComparison
        if (!email.Contains('@', StringComparison.Ordinal) || !email.Contains('.', StringComparison.Ordinal))
            throw new ArgumentException("Email must be in a valid format", nameof(email));
        
        Email = email;
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
