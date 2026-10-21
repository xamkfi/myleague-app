using Domain.Enums.Common;

namespace Application.Features.Common.Content.Advertisements.DTOs;

/// <summary>
/// Full advertisement data for the admin UI.
/// </summary>
public class AdvertisementDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? AltText { get; set; }
    public AdSlot Slot { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
