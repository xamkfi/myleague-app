using Application.DTOs.Common;
using Domain.Entities.Common;

namespace Application.Features.Common.RulesSection;

internal static class RulesSectionMapper
{
    public static RulesSectionDto ToDto(Domain.Entities.Common.RulesSection entity)
    {
        return new RulesSectionDto
        {
            Id = entity.Id,
            Title = entity.Title,
            SortOrder = entity.SortOrder,
            SectionType = entity.SectionType,
            ParentSectionId = entity.ParentSectionId,
            ContentHtml = entity.ContentHtml,
            LastModifiedBy = entity.LastModifiedBy,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }
}
