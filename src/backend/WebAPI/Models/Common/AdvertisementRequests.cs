using System.ComponentModel.DataAnnotations;
using Domain.Enums.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for creating or updating a home page advertisement.
/// </summary>
public class AdvertisementRequest
{
    /// <summary>Gets or sets the admin-facing name of the advertisement.</summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the uploaded image URL.</summary>
    [Required]
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional http(s) address opened when the ad is clicked.</summary>
    [StringLength(500)]
    public string? LinkUrl { get; set; }

    /// <summary>Gets or sets the image alt text.</summary>
    [StringLength(200)]
    public string? AltText { get; set; }

    /// <summary>Gets or sets the home page slot.</summary>
    [Required]
    public AdSlot Slot { get; set; }

    /// <summary>Gets or sets whether the advertisement may be shown.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the order within the slot.</summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    /// <summary>Gets or sets the optional UTC time the advertisement starts showing.</summary>
    public DateTime? StartsAt { get; set; }

    /// <summary>Gets or sets the optional UTC time the advertisement stops showing.</summary>
    public DateTime? EndsAt { get; set; }
}
