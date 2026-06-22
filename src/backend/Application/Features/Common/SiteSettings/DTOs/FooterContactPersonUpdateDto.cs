namespace Application.Features.Common.SiteSettings.DTOs;

/// <summary>
/// Footer contact person update payload.
/// </summary>
public record FooterContactPersonUpdateDto(
    string NameOrRole,
    string Email,
    string Phone
);
