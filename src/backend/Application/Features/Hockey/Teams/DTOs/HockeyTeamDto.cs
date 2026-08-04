namespace Application.Features.Hockey.Teams.DTOs;

/// <summary>
/// Data transfer object for a hockey team.
/// </summary>
public record HockeyTeamDto(
    Guid Id,
    string Name,
    string ShortName,
    Guid ClubId,
    Guid? DivisionId,
    string TeamCategory,
    string HomeArena,
    string PrimaryJerseyColor,
    string SecondaryJerseyColor,
    string? LogoUrl,
    bool IsActive,
    IReadOnlyCollection<HockeyTeamPlayerDto> Roster,
    IReadOnlyCollection<HockeyLineDto> Lines,
    IReadOnlyCollection<HockeyTeamStaffDto> StaffMembers);
