using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Mappings;
using Application.Features.Football.Seasons.Commands;
using Application.Features.Football.Seasons.DTOs;
using Domain.Entities.Common;
using Domain.Entities.Football.Competitions;
using Domain.Entities.Football.Matches;
using Domain.Entities.Football.Teams;
using Domain.ValueObjects.Football;

namespace Application.Features.Football.Seasons.Mappings;

/// <summary>
/// Mapper for FootballSeason entity.
/// </summary>
public static class FootballSeasonMapper
{
    public static IReadOnlyCollection<FootballSeasonDivisionDto> ToDivisionDtos(
        IEnumerable<FootballCompetitionDivision> seasonDivisions)
    {
        ArgumentNullException.ThrowIfNull(seasonDivisions);

        return seasonDivisions
            .Select(sd => new FootballSeasonDivisionDto(
                sd.DivisionId,
                sd.Teams.Count,
                sd.Teams.Select(t => t.TeamId).ToList().AsReadOnly()))
            .ToList()
            .AsReadOnly();
    }

    public static FootballSeasonDto ToDto(
        FootballCompetition season,
        IReadOnlyCollection<FootballSeasonDivisionDto> seasonDivisions,
        Dictionary<Guid, Club>? clubs = null,
        IEnumerable<FootballTeam>? seasonTeams = null,
        IEnumerable<FootballMatch>? seasonMatches = null)
    {
        ArgumentNullException.ThrowIfNull(season);
        ArgumentNullException.ThrowIfNull(seasonDivisions);

        IEnumerable<FootballTeam> teamsToMap = seasonTeams ?? season.Teams;
        IEnumerable<FootballMatch> matchesToMap = seasonMatches ?? season.Matches;

        FootballMatchRulesDto matchRulesDto = new(
            season.MatchRules.NumberOfHalves,
            season.MatchRules.HalfDurationMinutes,
            season.MatchRules.PlayersOnField,
            season.MatchRules.RequireGoalkeeper,
            season.MatchRules.MaxSubstitutions,
            season.MatchRules.RequireOfficialsToStart,
            season.MatchRules.AllowExtraTime,
            season.MatchRules.ExtraTimeHalfCount,
            season.MatchRules.ExtraTimeHalfDurationMinutes,
            season.MatchRules.AllowPenaltyShootout);

        FootballStandingRulesDto standingRulesDto = new(
            season.StandingRules.WinPoints,
            season.StandingRules.DrawPoints,
            season.StandingRules.LossPoints);

        return new FootballSeasonDto(
            season.Id,
            season.Name,
            season.StartDate.ToUniversalTime(),
            season.EndDate.ToUniversalTime(),
            season.IsActive,
            season.IsCompleted,
            seasonDivisions,
            ToTeamSummaryDtos(teamsToMap, clubs).ToList().AsReadOnly(),
            FootballMatchMapper.ToDtos(matchesToMap).ToList().AsReadOnly(),
            matchRulesDto,
            standingRulesDto,
            season.TeamCategory);
    }

    public static FootballSeason ToEntity(CreateFootballSeasonCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        DateTime startDateUtc = ToUtc(command.StartDate);
        DateTime endDateUtc = ToUtc(command.EndDate);

        FootballMatchRules matchRules = new(
            command.NumberOfHalves,
            command.HalfDurationMinutes,
            command.PlayersOnField,
            command.RequireGoalkeeper,
            command.MaxSubstitutions,
            command.RequireOfficialsToStart,
            command.AllowExtraTime,
            command.ExtraTimeHalfCount,
            command.ExtraTimeHalfDurationMinutes,
            command.AllowPenaltyShootout);

        FootballStandingRules standingRules = new(
            command.WinPoints,
            command.DrawPoints,
            command.LossPoints);

        return new FootballSeason(
            command.Name,
            startDateUtc,
            endDateUtc,
            matchRules,
            standingRules,
            command.TeamCategory);
    }

    public static void UpdateFromCommand(FootballCompetition season, UpdateFootballSeasonCommand command)
    {
        ArgumentNullException.ThrowIfNull(season);
        ArgumentNullException.ThrowIfNull(command);

        season.UpdateDetails(command.Name, ToUtc(command.StartDate), ToUtc(command.EndDate));

        FootballMatchRules matchRules = new(
            command.NumberOfHalves,
            command.HalfDurationMinutes,
            command.PlayersOnField,
            command.RequireGoalkeeper,
            command.MaxSubstitutions,
            command.RequireOfficialsToStart,
            command.AllowExtraTime,
            command.ExtraTimeHalfCount,
            command.ExtraTimeHalfDurationMinutes,
            command.AllowPenaltyShootout);
        season.UpdateMatchRules(matchRules);

        season.UpdateStandingRules(new FootballStandingRules(
            command.WinPoints,
            command.DrawPoints,
            command.LossPoints));

        if (command.TeamCategory.HasValue)
        {
            season.UpdateTeamCategory(command.TeamCategory.Value);
        }
    }

    private static IEnumerable<FootballTeamSummaryDto> ToTeamSummaryDtos(
        IEnumerable<FootballTeam> teams,
        Dictionary<Guid, Club>? clubs)
    {
        return teams.Select(team =>
        {
            Uri? clubLogo = null;
            if (clubs != null && clubs.TryGetValue(team.ClubId, out Club? club))
            {
                clubLogo = club.LogoUrl;
            }

            return new FootballTeamSummaryDto(
                team.Id,
                team.Name,
                team.ShortName,
                team.ClubId,
                team.GetEffectiveLogoUrl(clubLogo));
        });
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
