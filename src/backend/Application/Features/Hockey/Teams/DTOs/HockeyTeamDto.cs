namespace Application.Features.Hockey.Teams.DTOs;

/// <summary>
/// Data transfer object for a hockey team.
/// </summary>
/// <param name="Id">Unique identifier of the team</param>
/// <param name="Name">Full team name</param>
/// <param name="ShortName">Short display name</param>
/// <param name="ClubId">Owning club id</param>
/// <param name="DivisionId">Optional division id</param>
/// <param name="TeamCategory">Team category as a string</param>
/// <param name="HomeArena">Home arena name</param>
/// <param name="PrimaryJerseyColor">Primary jersey color</param>
/// <param name="SecondaryJerseyColor">Secondary jersey color</param>
/// <param name="LogoUrl">Optional logo URL</param>
/// <param name="IsActive">Whether the team is active</param>
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
    bool IsActive);
