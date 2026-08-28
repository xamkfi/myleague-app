using Domain.Enums.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Request model for creating a rules section
/// </summary>
public class CreateRulesSectionRequest
{
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
}

/// <summary>
/// Request model for updating a rules section
/// </summary>
public class UpdateRulesSectionRequest
{
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
}

/// <summary>
/// Request model for adding a rule to a rules section
/// </summary>
public class AddRulesSectionRuleRequest
{
    /// <summary>
    /// Gets or sets the rule HTML content
    /// </summary>
    public string RuleHtml { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating a rule within a rules section
/// </summary>
public class UpdateRulesSectionRuleRequest
{
    /// <summary>
    /// Gets or sets the updated rule HTML content
    /// </summary>
    public string RuleHtml { get; set; } = string.Empty;
}
