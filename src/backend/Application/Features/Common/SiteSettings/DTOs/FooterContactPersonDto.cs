namespace Application.Features.Common.SiteSettings.DTOs;

/// <summary>
/// Footer contact person response payload.
/// </summary>
public record FooterContactPersonDto(
    string NameOrRole,
    string Email,
    string Phone
);
