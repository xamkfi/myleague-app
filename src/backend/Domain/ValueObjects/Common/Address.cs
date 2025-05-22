using System;

namespace Domain.ValueObjects.Common;

/// <summary>
/// Represents a physical address as a value object
/// </summary>
public class Address : IEquatable<Address>
{
    /// <summary>
    /// Gets the street line 1
    /// </summary>
    public string Street1 { get; private set; }
    
    /// <summary>
    /// Gets the street line 2 (optional)
    /// </summary>
    public string? Street2 { get; private set; }
    
    /// <summary>
    /// Gets the city
    /// </summary>
    public string City { get; private set; }
    
    /// <summary>
    /// Gets the postal code
    /// </summary>
    public string PostalCode { get; private set; }
    
    /// <summary>
    /// Gets the country
    /// </summary>
    public string Country { get; private set; }

    private Address() 
    {
        Street1 = string.Empty;
        City = string.Empty;
        PostalCode = string.Empty;
        Country = string.Empty;
    }

    /// <summary>
    /// Creates a new address
    /// </summary>
    public Address(string street1, string city, string postalCode, string country, string? street2 = null)
    {
        ArgumentNullException.ThrowIfNull(street1);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(postalCode);
        ArgumentNullException.ThrowIfNull(country);
        
        if (string.IsNullOrWhiteSpace(street1))
            throw new ArgumentException("Street cannot be empty", nameof(street1));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));
        
        Street1 = street1;
        Street2 = street2;
        City = city;
        PostalCode = postalCode;
        Country = country;
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

    public static bool operator !=(Address? left, Address? right) => !(left == right);
} 
