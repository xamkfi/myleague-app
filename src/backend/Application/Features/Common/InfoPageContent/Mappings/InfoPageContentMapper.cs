using Application.DTOs.Common;

namespace Application.Features.Common.InfoPageContent.Mappings;

/// <summary>
/// Maps InfoPageContent entities to DTOs
/// </summary>
internal static class InfoPageContentMapper
{
    /// <summary>
    /// Maps an InfoPageContent entity to a DTO
    /// </summary>
    /// <param name="entity">The entity to map</param>
    /// <returns>The mapped DTO</returns>
    public static InfoPageContentDto ToDto(Domain.Entities.Common.InfoPageContent entity)
    {
        return new InfoPageContentDto
        {
            Id = entity.Id,
            PageSlug = entity.PageSlug,
            Title = entity.Title,
            ContentHtml = entity.ContentHtml,
            LastModifiedBy = entity.LastModifiedBy,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }
}
