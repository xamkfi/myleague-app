using Application.Features.Floorball.Tournaments.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Domain.Entities.Floorball.Tournament;
using Domain.Entities.Floorball;
using Domain.ValueObjects.Floorball;

namespace Application.Features.Floorball.Tournaments.Mappings;

/// <summary>
/// Mapper for FloorballTournament entities and related types
/// </summary>
public static class FloorballTournamentMapper
{
    /// <summary>
    /// Maps a FloorballTournament entity to a full FloorballTournamentDto
    /// </summary>
    /// <param name="tournament">The tournament entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when tournament is null</exception>
    public static FloorballTournamentDto ToDto(FloorballTournament tournament)
    {
        if (tournament == null)
            throw new ArgumentNullException(nameof(tournament));

        FloorballMatchRulesDto matchRulesDto = MapMatchRules(tournament.MatchRules);

        return new FloorballTournamentDto(
            tournament.Id,
            tournament.Name,
            tournament.DescriptionHtml,
            tournament.StartDate.ToUniversalTime(),
            tournament.EndDate.ToUniversalTime(),
            tournament.Location,
            tournament.Status.ToString(),
            tournament.PlayoffFormat.ToString(),
            tournament.GroupStageAdvancingCount,
            tournament.ImageUrls.Select(u => u.ToString()).ToList().AsReadOnly(),
            matchRulesDto,
            tournament.Groups
                .OrderBy(g => g.Phase)
                .ThenBy(g => g.SortOrder)
                .Select(ToGroupDto)
                .ToList()
                .AsReadOnly(),
            tournament.Matches.Select(MapMatchToDto).ToList().AsReadOnly());
    }

    /// <summary>
    /// Maps a FloorballTournament entity to a lightweight FloorballTournamentSummaryDto
    /// </summary>
    /// <param name="tournament">The tournament entity to map</param>
    /// <returns>The mapped summary DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when tournament is null</exception>
    public static FloorballTournamentSummaryDto ToSummaryDto(FloorballTournament tournament)
    {
        if (tournament == null)
            throw new ArgumentNullException(nameof(tournament));

        return new FloorballTournamentSummaryDto(
            tournament.Id,
            tournament.Name,
            tournament.StartDate.ToUniversalTime(),
            tournament.EndDate.ToUniversalTime(),
            tournament.Location,
            tournament.Status.ToString(),
            tournament.PlayoffFormat.ToString(),
            tournament.Groups.Count,
            tournament.Groups.Sum(g => g.Teams.Count));
    }

    /// <summary>
    /// Maps a FloorballTournamentGroup entity to a FloorballTournamentGroupDto
    /// </summary>
    /// <param name="group">The group entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when group is null</exception>
    public static FloorballTournamentGroupDto ToGroupDto(FloorballTournamentGroup group)
    {
        if (group == null)
            throw new ArgumentNullException(nameof(group));

        return new FloorballTournamentGroupDto(
            group.Id,
            group.TournamentId,
            group.Name,
            group.Phase.ToString(),
            group.SortOrder,
            group.Teams.Select(ToGroupTeamDto).ToList().AsReadOnly());
    }

    /// <summary>
    /// Maps a FloorballTournamentGroupTeam entity to a FloorballTournamentGroupTeamDto
    /// </summary>
    /// <param name="groupTeam">The group team entity to map</param>
    /// <returns>The mapped DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when groupTeam is null</exception>
    public static FloorballTournamentGroupTeamDto ToGroupTeamDto(FloorballTournamentGroupTeam groupTeam)
    {
        if (groupTeam == null)
            throw new ArgumentNullException(nameof(groupTeam));

        return new FloorballTournamentGroupTeamDto(
            groupTeam.Id,
            groupTeam.GroupId,
            groupTeam.TeamId,
            groupTeam.Team?.Name ?? "Unknown Team");
    }

    /// <summary>
    /// Maps a FloorballMatchRules value object to a FloorballMatchRulesDto
    /// </summary>
    private static FloorballMatchRulesDto MapMatchRules(FloorballMatchRules rules)
    {
        return new FloorballMatchRulesDto(
            rules.NumberOfPeriods,
            rules.PeriodDurationMinutes,
            rules.AllowOvertime,
            rules.OvertimeDurationMinutes,
            rules.AllowShootout);
    }

    /// <summary>
    /// Maps a FloorballMatch to a FloorballMatchDto with null-safe SeasonId handling
    /// for tournament matches that may not belong to a season.
    /// </summary>
    private static FloorballMatchDto MapMatchToDto(FloorballMatch match)
    {
        Dictionary<int, PeriodScoreDto> periodScores = match.PeriodScores
            .OrderBy(ps => ps.PeriodNumber)
            .ToDictionary(
                ps => ps.PeriodNumber,
                ps => new PeriodScoreDto(ps.HomeScore, ps.AwayScore, ps.IsCompleted));

        List<Guid> officials = match.Officials.Select(r => r.Id).ToList();

        List<FloorballGoalEventDto> goalEvents = match.GoalEvents
            .Select(g => new FloorballGoalEventDto(
                g.Id,
                g.TeamId,
                g.ScoringPlayerId ?? Guid.Empty,
                g.AssistingPlayerId,
                g.SecondaryAssistingPlayerId,
                g.PeriodNumber,
                g.TimeInSeconds,
                match.WentToOvertime,
                match.WentToShootout,
                "Unknown Player",
                "Unknown Player",
                "Unknown Player"))
            .ToList();

        List<FloorballPenaltyEventDto> penaltyEvents = match.PenaltyEvents
            .Select(p => new FloorballPenaltyEventDto(
                p.Id,
                p.TeamId,
                p.PlayerId,
                p.PenaltyType,
                p.DurationInMinutes,
                p.PeriodNumber,
                p.TimeInSeconds,
                p.Description ?? string.Empty,
                "Unknown Player"))
            .ToList();

        List<FloorballSaveEventDto> saveEvents = match.SaveEvents
            .Select(s => new FloorballSaveEventDto
            {
                Id = s.Id,
                TeamId = s.TeamId,
                GoalieId = s.GoalieId,
                PeriodNumber = s.PeriodNumber,
                TimeInSeconds = s.TimeInSeconds,
                WasInOvertime = s.WasInOvertime,
                WasInShootout = s.WasInShootout,
                GoalieName = "Unknown Player"
            })
            .ToList();

        FloorballMatchRulesDto matchRulesDto = MapMatchRules(match.MatchRules);

        return new FloorballMatchDto(
            match.Id,
            match.SeasonId,
            match.Season?.Name,
            match.TournamentId,
            match.Tournament?.Name,
            match.TournamentGroupId,
            match.TournamentRound?.ToString(),
            match.HomeTeamId,
            match.HomeTeam.Name,
            match.HomeTeam.GetEffectiveLogoUrl(null),
            match.AwayTeamId,
            match.AwayTeam.Name,
            match.AwayTeam.GetEffectiveLogoUrl(null),
            match.ScheduledDateTime.ToUniversalTime(),
            match.Venue,
            match.Status,
            match.HomeScore,
            match.AwayScore,
            match.WentToOvertime,
            match.WentToShootout,
            match.HomeActiveGoalieId,
            match.AwayActiveGoalieId,
            periodScores,
            officials,
            goalEvents,
            penaltyEvents,
            saveEvents,
            matchRulesDto);
    }
}
