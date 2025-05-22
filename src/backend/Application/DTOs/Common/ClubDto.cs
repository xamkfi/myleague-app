using System;
using System.Collections.Generic;

namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for Club entity
/// </summary>
public record ClubDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the club
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the club
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the founding date of the club
    /// </summary>
    public DateTime FoundingDate { get; set; }
    
    /// <summary>
    /// Gets or sets the city where the club is located
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the country where the club is located
    /// </summary>
    public string Country { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the website URL of the club
    /// </summary>
    public string WebsiteUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the logo URL of the club
    /// </summary>
    public string LogoUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the contact email of the club
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;
} 
