using System;

namespace Domain.ValueObjects.Common;

/// <summary>
/// Represents a physical address as a value object.
///
/// All fields — including <see cref="Country"/> — are optional. Tournament-imported player rosters
/// (PMT, ad-hoc events) don't carry address data, and demanding a country in those flows used to
/// surface as "Country is required" validation failures even though no address was supplied at all.
/// Callers that want to skip persisting an <see cref="Address"/> entirely should pass <c>null</c>
/// to the relevant Person setter; see <c>PersonMapper.ToAddress</c> for the "all-empty → null"
/// convention.
/// </summary>
public class Address : IEquatable<Address>
{
    /// <summary>
    /// Gets the street line 1
    /// </summary>
    public string? Street1 { get; private set; }

    /// <summary>
    /// Gets the street line 2 (optional)
    /// </summary>
    public string? Street2 { get; private set; }

    /// <summary>
    /// Gets the city
    /// </summary>
    public string? City { get; private set; }

    /// <summary>
    /// Gets the postal code
    /// </summary>
    public string? PostalCode { get; private set; }

    /// <summary>
    /// Gets the country (optional)
    /// </summary>
    public string? Country { get; private set; }

    private Address()
    {
        Street1 = null;
        City = null;
        PostalCode = null;
        Country = null;
    }

    /// <summary>
    /// Creates a new address. Every part is optional.
    /// </summary>
    public Address(string? street1, string? city, string? postalCode, string? country, string? street2 = null)
    {
        Street1 = string.IsNullOrWhiteSpace(street1) ? null : street1;
        Street2 = string.IsNullOrWhiteSpace(street2) ? null : street2;
        City = string.IsNullOrWhiteSpace(city) ? null : city;
        PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode;
        Country = string.IsNullOrWhiteSpace(country) ? null : country;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Address);
    }

    public bool Equals(Address? other)
    {
        if (other is null)
            return false;

        return Street1 == other.Street1 &&
               Street2 == other.Street2 &&
               City == other.City &&
               PostalCode == other.PostalCode &&
               Country == other.Country;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street1, Street2, City, PostalCode, Country);
    }

    public static bool operator ==(Address? left, Address? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !(left == right);
    }
} 
