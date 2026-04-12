using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Domain.Entities.Floorball;
using Domain.ValueObjects.Floorball;

namespace Application.Features.Floorball.Tournaments.Mappings;

/// <summary>
/// Mapper for FloorballTournament entity
/// </summary>
public static class FloorballTournamentMapper
{
    /// <summary>
    /// Maps a FloorballTournament entity to a FloorballTournamentDto
    /// </summary>
    public static FloorballTournamentDto ToDto(FloorballTournament tournament)
    {
        if (tournament == null)
            throw new ArgumentNullException(nameof(tournament));

        FloorballTournamentRulesDto rulesDto = ToRulesDto(tournament.TournamentRules);

        List<FloorballTournamentGroupDto> groupDtos = tournament.Groups
            .OrderBy(g => g.Order)
            .Select(ToGroupDto)
            .ToList();

        int teamCount = tournament.Groups.Sum(g => g.Teams.Count);
        int matchCount = tournament.Matches.Count;

        return new FloorballTournamentDto(
            tournament.Id,
            tournament.Name,
            tournament.StartDate.ToUniversalTime(),
            tournament.EndDate.ToUniversalTime(),
            tournament.IsActive,
            tournament.IsCompleted,
            tournament.ContentHtml,
            tournament.Venue,
            tournament.TournamentStatus.ToString(),
            rulesDto,
            groupDtos,
            teamCount,
            matchCount);
    }

    /// <summary>
    /// Maps a FloorballTournamentGroup entity to a FloorballTournamentGroupDto
    /// </summary>
    public static FloorballTournamentGroupDto ToGroupDto(FloorballTournamentGroup group)
    {
        if (group == null)
            throw new ArgumentNullException(nameof(group));

        List<FloorballTournamentGroupTeamDto> teamDtos = group.Teams
            .Select(ToGroupTeamDto)
            .ToList();

        return new FloorballTournamentGroupDto(
            group.Id,
            group.Name,
            group.Order,
            teamDtos);
    }

    /// <summary>
    /// Maps a FloorballTournamentGroupTeam entity to a FloorballTournamentGroupTeamDto
    /// </summary>
    public static FloorballTournamentGroupTeamDto ToGroupTeamDto(FloorballTournamentGroupTeam groupTeam)
    {
        if (groupTeam == null)
            throw new ArgumentNullException(nameof(groupTeam));

        string teamName = groupTeam.Team?.Name ?? "Unknown";

        return new FloorballTournamentGroupTeamDto(
            groupTeam.Id,
            groupTeam.TeamId,
            teamName);
    }

    /// <summary>
    /// Maps a FloorballTournamentRules value object to a FloorballTournamentRulesDto
    /// </summary>
    public static FloorballTournamentRulesDto ToRulesDto(FloorballTournamentRules rules)
    {
        if (rules == null)
            throw new ArgumentNullException(nameof(rules));

        FloorballMatchRulesDto groupStageRulesDto = new FloorballMatchRulesDto(
            rules.GroupStageMatchRules.NumberOfPeriods,
            rules.GroupStageMatchRules.PeriodDurationMinutes,
            rules.GroupStageMatchRules.AllowOvertime,
            rules.GroupStageMatchRules.OvertimeDurationMinutes,
            rules.GroupStageMatchRules.AllowShootout);

        FloorballMatchRulesDto playoffRulesDto = new FloorballMatchRulesDto(
            rules.PlayoffMatchRules.NumberOfPeriods,
            rules.PlayoffMatchRules.PeriodDurationMinutes,
            rules.PlayoffMatchRules.AllowOvertime,
            rules.PlayoffMatchRules.OvertimeDurationMinutes,
            rules.PlayoffMatchRules.AllowShootout);

        return new FloorballTournamentRulesDto(
            groupStageRulesDto,
            playoffRulesDto,
            rules.TeamsAdvancingPerGroup,
            rules.HasPlayoffStage,
            rules.HasThirdPlaceMatch);
    }
}
