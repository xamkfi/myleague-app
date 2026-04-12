using Application.Features.Common.SiteSettings.DTOs;

namespace Application.Features.Common.SiteSettings.Mappings;

/// <summary>
/// Maps footer contact value payloads to API DTOs.
/// </summary>
public static class FooterContactMapper
{
    /// <summary>
    /// Maps serialized setting value data to response DTO.
    /// </summary>
    public static FooterContactDto ToDto(FooterContactSettingValue value, string? lastModifiedBy, DateTime? updatedAt)
    {
        return new FooterContactDto(
            value.OrganizationName,
            value.OrganizationAddress,
            lastModifiedBy,
            updatedAt,
            value.ContactPersons);
    }
}
