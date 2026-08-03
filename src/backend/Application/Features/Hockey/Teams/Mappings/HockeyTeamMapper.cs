using Application.Features.Hockey.Teams.DTOs;
using Domain.Entities.Hockey.Teams;

namespace Application.Features.Hockey.Teams.Mappings;

public static class HockeyTeamMapper
{
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
