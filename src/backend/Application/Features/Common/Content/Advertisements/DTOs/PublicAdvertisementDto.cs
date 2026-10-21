using Domain.Enums.Common;

namespace Application.Features.Common.Content.Advertisements.DTOs;

/// <summary>
/// What the public home page needs to render an advertisement.
/// </summary>
public class PublicAdvertisementDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? AltText { get; set; }
    public AdSlot Slot { get; set; }
    public int SortOrder { get; set; }
}
