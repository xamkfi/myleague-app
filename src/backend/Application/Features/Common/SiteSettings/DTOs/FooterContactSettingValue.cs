namespace Application.Features.Common.SiteSettings.DTOs;

/// <summary>
/// Serialized value object stored in <c>SiteSetting.ValueJson</c> for footer contact information.
/// </summary>
public record FooterContactSettingValue(
    string OrganizationName,
    string OrganizationAddress,
    IReadOnlyList<FooterContactPersonDto> ContactPersons
);
