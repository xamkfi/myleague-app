using Application.DTOs.Common;
using Domain.Entities.Common;

namespace Application.Features.Common.RulesSection;

/// <summary>
/// Maps RulesSection entities to DTOs
/// </summary>
internal static class RulesSectionMapper
{
    /// <summary>
    /// Maps a RulesSection entity to a DTO
    /// </summary>
    /// <param name="entity">The entity to map</param>
    /// <returns>The mapped DTO</returns>
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
