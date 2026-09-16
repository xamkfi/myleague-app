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
    public static HockeyTeamDto ToDto(HockeyTeam team) => ToDto(team, null);

    /// <summary>
    /// Maps a hockey team to a DTO, optionally scoped to one competition roster.
    /// </summary>
    public static HockeyTeamDto ToDto(HockeyTeam team, Guid? competitionId)
    {
        IEnumerable<HockeyTeamPlayer> roster = competitionId.HasValue
            ? team.Roster.Where(p => p.CompetitionId == competitionId)
            : team.Roster;

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
            team.IsActive,
            roster.Select(ToTeamPlayerDto).ToList(),
            team.Lines.Select(ToLineDto).ToList(),
            team.StaffMembers.Select(ToStaffDto).ToList());
    }

    /// <summary>
    /// Maps a team-player membership to a DTO.
    /// </summary>
    public static HockeyTeamPlayerDto ToTeamPlayerDto(HockeyTeamPlayer teamPlayer)
    {
        return new HockeyTeamPlayerDto(
            teamPlayer.Id,
            teamPlayer.TeamId,
            teamPlayer.PlayerId,
            teamPlayer.CompetitionId,
            teamPlayer.Position.ToString(),
            teamPlayer.CaptainRole.ToString(),
            teamPlayer.RosterStatus.ToString(),
            teamPlayer.JerseyNumber,
            teamPlayer.RequestedJerseyNumber,
            teamPlayer.IsActive,
            teamPlayer.JoinedAt);
    }

    /// <summary>
    /// Maps a line to a DTO.
    /// </summary>
    public static HockeyLineDto ToLineDto(HockeyLine line)
    {
        return new HockeyLineDto(
            line.Id,
            line.TeamId,
            line.CompetitionId,
            line.Name,
            line.LineNumber,
            line.LineType.ToString(),
            line.IsActive,
            line.Players.Select(ToLinePlayerDto).ToList());
    }

    /// <summary>
    /// Maps a line-player assignment to a DTO.
    /// </summary>
    public static HockeyLinePlayerDto ToLinePlayerDto(HockeyLinePlayer linePlayer)
    {
        return new HockeyLinePlayerDto(
            linePlayer.Id,
            linePlayer.LineId,
            linePlayer.TeamPlayerId,
            linePlayer.Slot.ToString(),
            linePlayer.Order);
    }

    /// <summary>
    /// Maps a staff membership to a DTO.
    /// </summary>
    public static HockeyTeamStaffDto ToStaffDto(HockeyTeamStaff staff)
    {
        return new HockeyTeamStaffDto(
            staff.Id,
            staff.TeamId,
            staff.PersonId,
            staff.CompetitionId,
            staff.Role.ToString(),
            staff.IsActive,
            staff.JoinedAt);
    }
}
