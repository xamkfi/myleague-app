namespace Application.Features.Common.ClubAdmin.DTOs;

/// <summary>
/// A club managed by the current club admin, including all teams under the club.
/// </summary>
/// <param name="ClubId">The club ID</param>
/// <param name="Name">The club name</param>
/// <param name="City">The club city</param>
/// <param name="LogoUrl">Optional club logo URL</param>
/// <param name="Teams">The floorball and football teams belonging to the club</param>
public record ClubAdminClubDto(
    Guid ClubId,
    string Name,
    string City,
    string? LogoUrl,
    IReadOnlyList<ClubAdminTeamDto> Teams);
