using Application.DTOs.Common;
using Domain.Entities.Common;

namespace Application.Features.Common.FooterContacts.Mappings;

internal static class FooterContactMapper
{
    public static Uri? ParseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException("Url must be an http or https address", nameof(url));
        }

        return uri;
    }

    public static FooterContactDto ToDto(FooterContact entity)
    {
        return new FooterContactDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Details = entity.Details,
            Email = entity.Email,
            Phone = entity.Phone,
            Url = entity.Url?.ToString(),
            SortOrder = entity.SortOrder,
            Section = entity.Section,
            LastModifiedBy = entity.LastModifiedBy,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }
}
