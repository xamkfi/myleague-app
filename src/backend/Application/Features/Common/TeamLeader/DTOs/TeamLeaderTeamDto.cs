namespace Application.Features.Common.TeamLeader.DTOs;

/// <summary>
/// A team managed by the current team leader, including the sport it belongs to.
/// </summary>
/// <param name="Sport">Sport discriminator: "floorball" or "football"</param>
/// <param name="TeamId">The team ID</param>
/// <param name="Name">The team name</param>
/// <param name="ShortName">The team short name</param>
/// <param name="LogoUrl">Optional team logo URL</param>
public record TeamLeaderTeamDto(
    string Sport,
    Guid TeamId,
    string Name,
    string ShortName,
    string? LogoUrl);
