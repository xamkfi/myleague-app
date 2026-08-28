namespace Application.Features.Common.ClubAdmin.DTOs;

/// <summary>
/// A team belonging to a club managed by the current club admin.
/// </summary>
    /// <param name="Sport">Sport discriminator: "floorball", "football", or "hockey"</param>
/// <param name="TeamId">The team ID</param>
/// <param name="Name">The team name</param>
/// <param name="ShortName">The team short name</param>
/// <param name="LogoUrl">Optional team logo URL</param>
public record ClubAdminTeamDto(
    string Sport,
    Guid TeamId,
    string Name,
    string ShortName,
    string? LogoUrl);
