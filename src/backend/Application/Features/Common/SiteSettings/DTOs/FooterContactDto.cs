namespace Application.Features.Common.SiteSettings.DTOs;

/// <summary>
/// Footer contact response payload.
/// </summary>
public record FooterContactDto(
    string OrganizationName,
    string OrganizationAddress,
    string? LastModifiedBy,
    DateTime? UpdatedAt,
    IReadOnlyList<FooterContactPersonDto> ContactPersons
);
