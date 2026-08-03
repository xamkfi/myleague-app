using Application.Features.Hockey.Teams.DTOs;
using Domain.Entities.Hockey.Teams;

namespace Application.Features.Hockey.Teams.Mappings;

/// <summary>
/// Maps hockey team domain entities to application DTOs.
/// </summary>
public static class HockeyTeamMapper
{
    /// <summary>
    /// Maps a hockey team to a DTO.
    /// </summary>
    public static HockeyTeamDto ToDto(HockeyTeam team)
    {
        return new HockeyTeamDto(
            team.Id,
            team.Name,
            team.ShortName,
            team.ClubId,
            team.DivisionId,
            team.TeamCategory.ToString(),
            team.HomeArena,
            team.PrimaryJerseyColor,
            team.SecondaryJerseyColor,
            team.LogoUrl?.ToString(),
            team.IsActive);
    }
}
