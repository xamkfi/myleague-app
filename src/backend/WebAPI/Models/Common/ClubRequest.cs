using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for retrieving clubs with pagination
/// </summary>
public record GetClubsRequest
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
}

/// <summary>
/// Request model for creating a new club
/// </summary>
public record CreateClubRequest
{
    /// <summary>
    /// Gets the name of the club
    /// </summary>
    [Required(ErrorMessage = "Club name is required")]
    [StringLength(100, ErrorMessage = "Club name cannot exceed 100 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the city where the club is located
    /// </summary>
    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
    public string City { get; init; } = string.Empty;

    /// <summary>
    /// Gets the country where the club is located
    /// </summary>
    [Required(ErrorMessage = "Country is required")]
    [StringLength(50, ErrorMessage = "Country name cannot exceed 50 characters")]
    public string Country { get; init; } = string.Empty;

    /// <summary>
    /// Gets the founding date of the club
    /// </summary>
    [Required(ErrorMessage = "Founding date is required")]
    public DateTime FoundingDate { get; init; }

    /// <summary>
    /// Gets the website URL of the club
    /// </summary>
    [Url(ErrorMessage = "Please provide a valid website URL")]
    [StringLength(200, ErrorMessage = "Website URL cannot exceed 200 characters")]
    public string? WebsiteUrl { get; init; }

    /// <summary>
    /// Gets the logo URL of the club
    /// </summary>
    [Url(ErrorMessage = "Please provide a valid logo URL")]
    [StringLength(200, ErrorMessage = "Logo URL cannot exceed 200 characters")]
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Gets the contact email address of the club
    /// </summary>
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(100, ErrorMessage = "Contact email cannot exceed 100 characters")]
    public string? ContactEmail { get; init; }
}

/// <summary>
/// Request model for updating an existing club
/// </summary>
public record UpdateClubRequest
{
    /// <summary>
    /// Gets the name of the club
    /// </summary>
    [Required(ErrorMessage = "Club name is required")]
    [StringLength(100, ErrorMessage = "Club name cannot exceed 100 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the city where the club is located
    /// </summary>
    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
    public string City { get; init; } = string.Empty;

    /// <summary>
    /// Gets the country where the club is located
    /// </summary>
    [Required(ErrorMessage = "Country is required")]
    [StringLength(50, ErrorMessage = "Country name cannot exceed 50 characters")]
    public string Country { get; init; } = string.Empty;

    /// <summary>
    /// Gets the founding date of the club
    /// </summary>
    [Required(ErrorMessage = "Founding date is required")]
    public DateTime FoundingDate { get; init; }

    /// <summary>
    /// Gets the website URL of the club
    /// </summary>
    [Url(ErrorMessage = "Please provide a valid website URL")]
    [StringLength(200, ErrorMessage = "Website URL cannot exceed 200 characters")]
    public string? WebsiteUrl { get; init; }

    /// <summary>
    /// Gets the logo URL of the club
    /// </summary>
    [Url(ErrorMessage = "Please provide a valid logo URL")]
    [StringLength(200, ErrorMessage = "Logo URL cannot exceed 200 characters")]
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Gets the contact email address of the club
    /// </summary>
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(100, ErrorMessage = "Contact email cannot exceed 100 characters")]
    public string? ContactEmail { get; init; }
} 
