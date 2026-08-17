using Application.Features.Football.Tournaments.DTOs;
using Application.Features.Football.Matches.DTOs;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.Entities.Football.Statistics;
using Domain.ValueObjects.Football;

namespace Application.Features.Football.Tournaments.Mappings;

/// <summary>
/// Mapper for FootballTournament entity
/// </summary>
public static class FootballTournamentMapper
{
    /// <summary>
    /// Maps a FootballTournament entity to a FootballTournamentDto
    /// </summary>
    public static FootballTournamentDto ToDto(FootballTournament tournament)
    {
        if (tournament == null)
            throw new ArgumentNullException(nameof(tournament));

        FootballTournamentRulesDto rulesDto = ToRulesDto(tournament.TournamentRules);

        List<FootballTournamentGroupDto> groupDtos = tournament.Groups
            .OrderBy(g => g.Order)
            .Select(ToGroupDto)
            .ToList();

        int teamCount = tournament.Groups.Sum(g => g.Teams.Count);
        int matchCount = tournament.Matches.Count;

        List<FootballPlayoffScheduleSlotDto> playoffSlots = tournament.PlayoffSchedule
            .OrderBy(s => s.Round)
            .ThenBy(s => s.Order)
            .Select(s => new FootballPlayoffScheduleSlotDto(
                s.Round,
                s.Order,
                DateTime.SpecifyKind(s.ScheduledDateTime, DateTimeKind.Utc),
                s.Venue))
            .ToList();

        return new FootballTournamentDto(
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
            matchCount,
            playoffSlots,
            tournament.TeamCategory);
    }

    /// <summary>
    /// Maps a FootballTournamentGroup entity to a FootballTournamentGroupDto
    /// </summary>
    public static FootballTournamentGroupDto ToGroupDto(FootballTournamentGroup group)
    {
        if (group == null)
            throw new ArgumentNullException(nameof(group));

        List<FootballTournamentGroupTeamDto> teamDtos = group.Teams
            .Select(ToGroupTeamDto)
            .ToList();

        return new FootballTournamentGroupDto(
            group.Id,
            group.Name,
            group.Order,
            teamDtos);
    }

    /// <summary>
    /// Maps a FootballTournamentGroupTeam entity to a FootballTournamentGroupTeamDto
    /// </summary>
    public static FootballTournamentGroupTeamDto ToGroupTeamDto(FootballTournamentGroupTeam groupTeam)
    {
        if (groupTeam == null)
            throw new ArgumentNullException(nameof(groupTeam));

        string teamName = groupTeam.Team?.Name ?? "Unknown";

        return new FootballTournamentGroupTeamDto(
            groupTeam.Id,
            groupTeam.TeamId,
            teamName);
    }

    /// <summary>
    /// Maps a FootballTournamentRules value object to a FootballTournamentRulesDto
    /// </summary>
    public static FootballTournamentRulesDto ToRulesDto(FootballTournamentRules rules)
    {
        if (rules == null)
            throw new ArgumentNullException(nameof(rules));

        FootballMatchRulesDto groupStageRulesDto = new FootballMatchRulesDto(
            rules.GroupStageMatchRules.NumberOfHalves,
            rules.GroupStageMatchRules.HalfDurationMinutes,
            rules.GroupStageMatchRules.PlayersOnField,
            rules.GroupStageMatchRules.RequireGoalkeeper,
            rules.GroupStageMatchRules.MaxSubstitutions,
            rules.GroupStageMatchRules.RequireOfficialsToStart,
            rules.GroupStageMatchRules.AllowExtraTime,
            rules.GroupStageMatchRules.ExtraTimeHalfCount,
            rules.GroupStageMatchRules.ExtraTimeHalfDurationMinutes,
            rules.GroupStageMatchRules.AllowPenaltyShootout);

        FootballMatchRulesDto playoffRulesDto = new FootballMatchRulesDto(
            rules.PlayoffMatchRules.NumberOfHalves,
            rules.PlayoffMatchRules.HalfDurationMinutes,
            rules.PlayoffMatchRules.PlayersOnField,
            rules.PlayoffMatchRules.RequireGoalkeeper,
            rules.PlayoffMatchRules.MaxSubstitutions,
            rules.PlayoffMatchRules.RequireOfficialsToStart,
            rules.PlayoffMatchRules.AllowExtraTime,
            rules.PlayoffMatchRules.ExtraTimeHalfCount,
            rules.PlayoffMatchRules.ExtraTimeHalfDurationMinutes,
            rules.PlayoffMatchRules.AllowPenaltyShootout);

        return new FootballTournamentRulesDto(
            groupStageRulesDto,
            playoffRulesDto,
            rules.TeamsAdvancingPerGroup,
            rules.HasPlayoffStage,
            rules.HasThirdPlaceMatch);
    }
}
