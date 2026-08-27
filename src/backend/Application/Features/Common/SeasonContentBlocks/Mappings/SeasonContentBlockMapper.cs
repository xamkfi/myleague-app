using Application.DTOs.Common;
using Domain.Entities.Common;

namespace Application.Features.Common.SeasonContentBlocks.Mappings;

/// <summary>
/// Maps SeasonContentBlock entities to DTOs
/// </summary>
internal static class SeasonContentBlockMapper
{
    /// <summary>
    /// Maps a SeasonContentBlock entity to a DTO
    /// </summary>
    public static SeasonContentBlockDto ToDto(SeasonContentBlock entity)
    {
        return new SeasonContentBlockDto
        {
            Id = entity.Id,
            Sport = entity.Sport,
            CompetitionId = entity.CompetitionId,
            SeasonYear = entity.SeasonYear,
            Title = entity.Title,
            ContentHtml = entity.ContentHtml,
            SortOrder = entity.SortOrder,
            LastModifiedBy = entity.LastModifiedBy,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }
}
