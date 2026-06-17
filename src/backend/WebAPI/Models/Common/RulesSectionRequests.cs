using Domain.Enums.Common;

namespace WebAPI.Models.Common;

public class CreateRulesSectionRequest
{
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public RulesSectionType SectionType { get; set; }
    public Guid? ParentSectionId { get; set; }
}

public class UpdateRulesSectionRequest
{
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public RulesSectionType SectionType { get; set; }
    public Guid? ParentSectionId { get; set; }
}

public class AddRulesSectionRuleRequest
{
    public string RuleHtml { get; set; } = string.Empty;
}

public class UpdateRulesSectionRuleRequest
{
    public string RuleHtml { get; set; } = string.Empty;
}
