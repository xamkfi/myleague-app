using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Address information for person requests
/// </summary>
public record AddressRequest
{
    /// <summary>
    /// Gets the street address (line 1) of the person
    /// </summary>
    [Required(ErrorMessage = "Street address is required")]
    [StringLength(100, ErrorMessage = "Street address cannot exceed 100 characters")]
    public string Street1 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional street address (line 2) of the person
    /// </summary>
    [StringLength(100, ErrorMessage = "Street address line 2 cannot exceed 100 characters")]
    public string? Street2 { get; init; }

    /// <summary>
    /// Gets the city of the person
    /// </summary>
    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    public string City { get; init; } = string.Empty;

    /// <summary>
    /// Gets the postal code of the person
    /// </summary>
    [Required(ErrorMessage = "Postal code is required")]
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string PostalCode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the country of the person
    /// </summary>
    [Required(ErrorMessage = "Country is required")]
    [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
    public string Country { get; init; } = string.Empty;
}

/// <summary>
/// Contact information for person requests
/// </summary>
public record ContactInfoRequest
{
    /// <summary>
    /// Gets the email address of the person
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the phone number of the person
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Please provide a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string Phone { get; init; } = string.Empty;

    /// <summary>
    /// Gets the alternative phone number of the person
    /// </summary>
    [Phone(ErrorMessage = "Please provide a valid phone number")]
    [StringLength(20, ErrorMessage = "Alternative phone number cannot exceed 20 characters")]
    public string? AlternativePhone { get; init; }
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
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the birth date of the person
    /// </summary>
    [Required(ErrorMessage = "Birth date is required")]
    public DateTime BirthDate { get; init; }

    /// <summary>
    /// Gets the address information of the person
    /// </summary>
    [Required(ErrorMessage = "Address information is required")]
    public AddressRequest Address { get; init; } = new();

    /// <summary>
    /// Gets the contact information of the person
    /// </summary>
    [Required(ErrorMessage = "Contact information is required")]
    public ContactInfoRequest ContactInfo { get; init; } = new();
}

/// <summary>
/// Request model for updating an existing person
/// </summary>
public record UpdatePersonRequest
{
    /// <summary>
    /// Gets the first name of the person
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name of the person
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the birth date of the person
    /// </summary>
    [Required(ErrorMessage = "Birth date is required")]
    public DateTime BirthDate { get; init; }

    /// <summary>
    /// Gets the address information of the person
    /// </summary>
    [Required(ErrorMessage = "Address information is required")]
    public AddressRequest Address { get; init; } = new();

    /// <summary>
    /// Gets the contact information of the person
    /// </summary>
    [Required(ErrorMessage = "Contact information is required")]
    public ContactInfoRequest ContactInfo { get; init; } = new();
} 