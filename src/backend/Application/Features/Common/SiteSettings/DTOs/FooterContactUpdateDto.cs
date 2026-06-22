namespace Application.Features.Common.SiteSettings.DTOs;

/// <summary>
/// Footer contact update payload.
/// </summary>
public record FooterContactUpdateDto(
    string OrganizationName,
    string OrganizationAddress,
    IReadOnlyList<FooterContactPersonUpdateDto> ContactPersons
);
