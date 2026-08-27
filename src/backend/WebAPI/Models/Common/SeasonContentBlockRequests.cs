using Domain.Enums.Common;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for creating a season content block
/// </summary>
public class CreateSeasonContentBlockRequest
{
    /// <summary>
    /// Gets or sets the sport this block belongs to
    /// </summary>
    [Required]
    public SportsCategory Sport { get; set; }

    /// <summary>
    /// Gets or sets the season (competition) this block is attached to
    /// </summary>
    [Required]
    public Guid CompetitionId { get; set; }

    /// <summary>
    /// Gets or sets the season-year label used on the public sport page
    /// </summary>
    [Required]
    [StringLength(32)]
    public string SeasonYear { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the block title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rich-text HTML body
    /// </summary>
    [Required]
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display sort order
    /// </summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}

/// <summary>
/// Request model for updating a season content block
/// </summary>
public class UpdateSeasonContentBlockRequest
{
    /// <summary>
    /// Gets or sets the block title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rich-text HTML body
    /// </summary>
    [Required]
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display sort order
    /// </summary>
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}

/// <summary>
/// Request model for reordering season content blocks
/// </summary>
public class ReorderSeasonContentBlocksRequest
{
    /// <summary>
    /// Gets or sets the block IDs in the desired display order
    /// </summary>
    [Required]
    public List<Guid> OrderedIds { get; set; } = [];
}
