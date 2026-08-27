using Domain.Enums.Common;

namespace Application.DTOs.Common;

/// <summary>
/// Data transfer object for a season content block
/// </summary>
public class SeasonContentBlockDto
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sport this block belongs to
    /// </summary>
    public SportsCategory Sport { get; set; }

    /// <summary>
    /// Gets or sets the season (competition) this block is attached to
    /// </summary>
    public Guid CompetitionId { get; set; }

    /// <summary>
    /// Gets or sets the season-year label used on the public sport page (e.g. "2025-2026")
    /// </summary>
    public string SeasonYear { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the block title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rich-text HTML body
    /// </summary>
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display sort order (ascending)
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the username of the last modifier
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
