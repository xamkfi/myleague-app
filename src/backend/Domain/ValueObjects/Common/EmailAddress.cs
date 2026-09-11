namespace Domain.ValueObjects.Common;

/// <summary>
/// Normalizes email addresses for storage and lookup.
/// Case is treated as insignificant so login and uniqueness stay consistent.
/// </summary>
public static class EmailAddress
{
    /// <summary>
    /// Trims and lowercases a required email. Throws when the value is missing.
    /// </summary>
    public static string Normalize(string email)
    {
        string? normalized = NormalizeOptional(email);
        if (normalized is null)
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        }

        return normalized;
    }

    /// <summary>
    /// Trims and lowercases an optional email. Returns null when the value is missing.
    /// </summary>
    public static string? NormalizeOptional(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

#pragma warning disable CA1308 // Emails are stored lowercase so login and uniqueness stay consistent.
        return email.Trim().ToLowerInvariant();
#pragma warning restore CA1308
    }
}
