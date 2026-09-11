using Domain.Enums.Common;

namespace Application.DTOs.Common;

/// <summary>
/// Data Transfer Object for RulesSection entity
/// </summary>
public class RulesSectionDto
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the section title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display sort order
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the section type
    /// </summary>
    public RulesSectionType SectionType { get; set; }

    /// <summary>
    /// Gets or sets the parent section ID, if nested
    /// </summary>
    public Guid? ParentSectionId { get; set; }

    /// <summary>
    /// Gets or sets the section HTML content containing rules
    /// </summary>
    public string ContentHtml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the last modifier
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
