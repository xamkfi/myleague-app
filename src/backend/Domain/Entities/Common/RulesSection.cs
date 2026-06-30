using Domain.Enums.Common;

namespace Domain.Entities.Common;

/// <summary>
/// Represents a rules section (tab or sport sub-section) with HTML rule content.
/// </summary>
public class RulesSection : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public RulesSectionType SectionType { get; private set; }
    public Guid? ParentSectionId { get; private set; }
    public RulesSection? ParentSection { get; private set; }
    public ICollection<RulesSection> ChildSections { get; private set; } = new List<RulesSection>();
    public string ContentHtml { get; private set; } = string.Empty;
    public string? LastModifiedBy { get; private set; }

    private RulesSection() { }

    public RulesSection(
        Guid id,
        string title,
        int sortOrder,
        RulesSectionType sectionType,
        Guid? parentSectionId = null,
        string contentHtml = "",
        string? lastModifiedBy = null)
    {
        Id = id;
        Title = ValidateTitle(title);
        SortOrder = sortOrder;
        SectionType = sectionType;
        ParentSectionId = parentSectionId;
        ContentHtml = contentHtml ?? string.Empty;
        LastModifiedBy = lastModifiedBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateMetadata(
        string title,
        int sortOrder,
        RulesSectionType sectionType,
        Guid? parentSectionId,
        string? lastModifiedBy = null)
    {
        Title = ValidateTitle(title);
        SortOrder = sortOrder;
        SectionType = sectionType;
        ParentSectionId = parentSectionId;
        LastModifiedBy = lastModifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContentHtml(string contentHtml, string? lastModifiedBy = null)
    {
        ContentHtml = contentHtml ?? string.Empty;
        LastModifiedBy = lastModifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Section title cannot be empty", nameof(title));
        }

        if (title.Length > 200)
        {
            throw new ArgumentException("Section title cannot exceed 200 characters", nameof(title));
        }

        return title.Trim();
    }
}
