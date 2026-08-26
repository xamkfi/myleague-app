using Application.DTOs.Common;
using Domain.Entities.Common;

namespace Application.Features.Common.FooterContacts.Mappings;

internal static class FooterContactMapper
{
    public static FooterContactDto ToDto(FooterContact entity)
    {
        return new FooterContactDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Details = entity.Details,
            Email = entity.Email,
            Phone = entity.Phone,
            Url = entity.Url,
            SortOrder = entity.SortOrder,
            LastModifiedBy = entity.LastModifiedBy,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }
}
