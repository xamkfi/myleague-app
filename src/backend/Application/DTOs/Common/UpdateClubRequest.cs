using System;

namespace Application.DTOs.Common;

/// <summary>
/// Request DTO for updating an existing club
/// </summary>
public record UpdateClubRequest
{
    /// <summary>
    /// Gets or sets the name of the club
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the founding date of the club
    /// </summary>
    public DateTime FoundingDate { get; init; }
    
    /// <summary>
    /// Gets or sets the city where the club is located
    /// </summary>
    public string City { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the country where the club is located
    /// </summary>
    public string Country { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the website URL of the club
    /// </summary>
    public string WebsiteUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the logo URL of the club
    /// </summary>
    public string LogoUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the contact email of the club
    /// </summary>
    public string ContactEmail { get; init; } = string.Empty;
} 