using Domain.Enums.Common;

namespace Application.DTOs.Common;

public class RulesSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public RulesSectionType SectionType { get; set; }
    public Guid? ParentSectionId { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
