using System.ComponentModel.DataAnnotations;
using Application.DTOs.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for retrieving persons
/// </summary>
public record GetPersonsRequest
{

    /// <summary>
    /// Gets the page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets the number of items per page (0 means use default)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Page size must be between 0 and 100")]
    public int PageSize { get; init; } = 25;

    /// <summary>
    /// Gets the first name of the person
    /// </summary>
    public string? FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    public string? LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the birth date of the person
    /// </summary>
    public string? BirthDate { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether the person is registered
    /// </summary>
    public bool? IsRegistered { get; init; } = null;
}

/// <summary>
/// Request model for creating a new person
/// </summary>
public record CreatePersonRequest
{
    /// <summary>
    /// Gets the first name of the person
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the birth date of the person
    /// </summary>
    public string? BirthDate { get; init; } = null;

    /// <summary>
    /// Gets whether the person is registered
    /// </summary>
    public bool IsRegistered { get; init; } = false;

    /// <summary>
    /// Gets the address of the person (optional)
    /// </summary>
    public AddressDto? Address { get; init; }

    /// <summary>
    /// Gets the contact information of the person (optional)
    /// </summary>
    public ContactInfoDto? ContactInfo { get; init; }
}

/// <summary>
/// Request model for updating an existing person's basic information
/// </summary>
public record UpdatePersonBasicInfoRequest
{
    /// <summary>
    /// Gets the first name of the person
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string LastName { get; init; } = string.Empty;
}

/// <summary>
/// Request model for updating an existing person's address
/// </summary>
public record UpdatePersonAddressRequest
{
    /// <summary>
    /// Gets the street address line 1
    /// </summary>
    [StringLength(200, ErrorMessage = "Street address cannot exceed 200 characters")]
    public string? Street1 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the street address line 2
    /// </summary>
    [StringLength(200, ErrorMessage = "Street address 2 cannot exceed 200 characters")]
    public string? Street2 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the city
    /// </summary>
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; init; } = string.Empty;

    /// <summary>
    /// Gets the postal code
    /// </summary>
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string? PostalCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the country
    /// </summary>
    [Required(ErrorMessage = "Country is required")]
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string Country { get; init; } = string.Empty;
}

/// <summary>
/// Request model for updating an existing person's contact information
/// </summary>
public record UpdatePersonContactInfoRequest
{
    /// <summary>
    /// Gets the email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the phone number
    /// </summary>
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? Phone { get; init; } = string.Empty;

    /// <summary>
    /// Gets the alternative phone number
    /// </summary>
    [StringLength(50, ErrorMessage = "Alternative phone number cannot exceed 50 characters")]
    public string? AlternativePhone { get; init; } = string.Empty;
}

/// <summary>
/// Request model for updating a complete person
/// </summary>
public record UpdatePersonRequest
{
    /// <summary>
    /// Gets the first name of the person
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the birth date of the person
    /// </summary>
    public DateTime? BirthDate { get; init; }

    /// <summary>
    /// Gets whether the person is registered
    /// </summary>
    public bool IsRegistered { get; init; } = false;

    /// <summary>
    /// Gets the address of the person (optional)
    /// </summary>
    public AddressDto? Address { get; init; }

    /// <summary>
    /// Gets the contact information of the person (optional)
    /// </summary>
    public ContactInfoDto? ContactInfo { get; init; }
} 
