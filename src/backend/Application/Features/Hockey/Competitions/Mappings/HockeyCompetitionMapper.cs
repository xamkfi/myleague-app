using Application.Features.Hockey.Competitions.DTOs;
using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;
using Domain.Entities.Hockey.Competitions;
using Domain.Entities.Hockey.Matches;
using Domain.Enums.Hockey.Matches;
using Domain.ValueObjects.Hockey.Matches;
using Domain.ValueObjects.Hockey.Rules;

namespace Application.Features.Hockey.Competitions.Mappings;

/// <summary>
/// Maps hockey competition domain entities to application DTOs.
/// </summary>
public static class HockeyCompetitionMapper
{
    /// <summary>
    /// Maps a hockey competition (season or tournament) to a shared summary DTO.
    /// </summary>
    public static HockeyCompetitionDto ToCompetitionDto(HockeyCompetition competition)
    {
        return new HockeyCompetitionDto(
            competition.Id,
            competition.Name,
            competition.CompetitionType.ToString(),
            competition.StartDate,
            competition.EndDate,
            competition.Status.ToString(),
            competition.IsActive,
            competition.IsCompleted,
            competition.Teams.Select(ToTeamDto).ToList(),
            competition.Divisions.Select(ToDivisionDto).ToList(),
            competition.PlayoffSeries.Select(series => ToPlayoffSeriesDto(series, competition.Matches)).ToList());
    }

    /// <summary>
    /// Maps competition rules to a full DTO.
    /// </summary>
    public static HockeyCompetitionRulesDto ToCompetitionRulesDto(HockeyCompetitionRules rules)
    {
        return new HockeyCompetitionRulesDto(
            rules.Name,
            rules.RuleBookVersion,
            rules.RuleBookSource.ToString(),
            ToMatchRulesDto(rules.MatchRules),
            ToStandingRulesDto(rules.StandingRules),
            ToRosterRulesDto(rules.RosterRules),
            rules.VideoReviewRules is null ? null : ToVideoReviewRulesDto(rules.VideoReviewRules),
            rules.ContactRules is null ? null : ToContactRulesDto(rules.ContactRules));
    }

    public static HockeyMatchRulesDto ToMatchRulesDto(HockeyMatchRules rules) =>
        new(
            rules.RegularPeriodCount,
            rules.RegularPeriodLengthMinutes,
            rules.OvertimeLengthMinutes,
            rules.StopClock,
            rules.OvertimeEnabled,
            rules.ShootoutEnabled,
            rules.OffsideEnabled,
            rules.DelayedOffsideEnabled,
            rules.IcingRule.ToString(),
            rules.PenaltyShotEnabled,
            rules.GoaliePullAllowed);

    public static HockeyStandingRulesDto ToStandingRulesDto(HockeyStandingRules rules) =>
        new(
            rules.RegulationWinPoints,
            rules.OvertimeWinPoints,
            rules.ShootoutWinPoints,
            rules.OvertimeLossPoints,
            rules.ShootoutLossPoints,
            rules.TiePoints,
            rules.TieBreakers.Select(t => t.ToString()).ToList());

    public static HockeyRosterRulesDto ToRosterRulesDto(HockeyRosterRules rules) =>
        new(
            rules.MaxDressedPlayers,
            rules.MaxDressedGoalies,
            rules.MinDressedPlayers,
            rules.RequiresGoalie,
            rules.MaxCaptains,
            rules.MaxAlternateCaptains,
            rules.CanGoalieBeCaptain,
            rules.AllowGuestPlayers,
            rules.LineManagementEnabled);

    public static HockeyVideoReviewRulesDto ToVideoReviewRulesDto(HockeyVideoReviewRules rules) =>
        new(
            rules.Enabled,
            rules.CoachChallengeAllowed,
            rules.ReviewGoals,
            rules.ReviewOffsideBeforeGoal,
            rules.ReviewGoalieInterference,
            rules.ReviewHighStickGoal,
            rules.ReviewPuckOverLine,
            new HockeyCoachChallengeRulesDto(
                rules.CoachChallengeRules.Enabled,
                rules.CoachChallengeRules.MaxChallengesPerTeam,
                rules.CoachChallengeRules.LoseChallengeAfterFailed,
                rules.CoachChallengeRules.PenaltyForFailedChallenge,
                rules.CoachChallengeRules.FailedChallengePenaltyMinutes,
                rules.CoachChallengeRules.FailedChallengePenaltyOffence.ToString(),
                rules.CoachChallengeRules.FailedChallengePenaltySeverity.ToString(),
                rules.CoachChallengeRules.AllowChallengeInOvertime,
                rules.CoachChallengeRules.AllowChallengeInShootout));

    public static HockeyContactRulesDto ToContactRulesDto(HockeyContactRules rules) =>
        new(
            rules.BodyCheckingAllowed,
            rules.OpenIceHitsAllowed,
            rules.FightingAllowed,
            rules.AutomaticGameMisconductForFight,
            rules.StrictHeadContactRule);

    /// <summary>
    /// Maps a competition-team membership to a DTO.
    /// </summary>
    public static HockeyCompetitionTeamDto ToTeamDto(HockeyCompetitionTeam team)
    {
        return new HockeyCompetitionTeamDto(
            team.Id,
            team.CompetitionId,
            team.TeamId,
            team.Seed,
            team.JoinedAt,
            team.IsActive);
    }

    /// <summary>
    /// Maps a hockey season to a DTO.
    /// </summary>
    public static HockeySeasonDto ToSeasonDto(HockeySeason season)
    {
        return new HockeySeasonDto(
            season.Id,
            season.Name,
            season.StartDate,
            season.EndDate,
            season.Status.ToString(),
            season.IsActive,
            season.IsCompleted,
            season.SeasonCode,
            season.TeamCategory.ToString(),
            season.ChampionCompetitionTeamId,
            season.Teams.Select(ToTeamDto).ToList(),
            season.Divisions.Select(ToDivisionDto).ToList(),
            season.PlayoffSeries.Select(series => ToPlayoffSeriesDto(series, season.Matches)).ToList(),
            season.PlayoffSchedule.Select(ToPlayoffScheduleSlotDto).ToList());
    }

    /// <summary>
    /// Maps a competition division to a DTO.
    /// </summary>
    public static HockeyCompetitionDivisionDto ToDivisionDto(HockeyCompetitionDivision division)
    {
        return new HockeyCompetitionDivisionDto(
            division.Id,
            division.CompetitionId,
            division.DivisionId,
            division.Name,
            division.SortOrder,
            division.IsActive,
            division.ChampionCompetitionTeamId,
            division.Teams.Select(ToDivisionTeamDto).ToList());
    }

    /// <summary>
    /// Maps a competition division team membership to a DTO.
    /// </summary>
    public static HockeyCompetitionDivisionTeamDto ToDivisionTeamDto(HockeyCompetitionDivisionTeam team)
    {
        return new HockeyCompetitionDivisionTeamDto(
            team.Id,
            team.CompetitionDivisionId,
            team.CompetitionTeamId,
            team.Seed,
            team.IsActive);
    }

    /// <summary>
    /// Maps a hockey tournament to a DTO.
    /// </summary>
    public static HockeyTournamentDto ToTournamentDto(HockeyTournament tournament)
    {
        return new HockeyTournamentDto(
            tournament.Id,
            tournament.Name,
            tournament.StartDate,
            tournament.EndDate,
            tournament.Status.ToString(),
            tournament.IsActive,
            tournament.IsCompleted,
            tournament.Venue,
            tournament.ContentHtml,
            tournament.CurrentStage.ToString(),
            tournament.TeamCategory.ToString(),
            tournament.ChampionCompetitionTeamId,
            tournament.Teams.Select(ToTeamDto).ToList(),
            tournament.Groups.Select(ToGroupDto).ToList(),
            tournament.PlayoffSeries.Select(series => ToPlayoffSeriesDto(series, tournament.Matches)).ToList(),
            ToTournamentRulesDto(tournament.TournamentRules),
            tournament.PlayoffSchedule.Select(ToPlayoffScheduleSlotDto).ToList());
    }

    /// <summary>
    /// Maps a tournament group to a DTO.
    /// </summary>
    public static HockeyTournamentGroupDto ToGroupDto(HockeyTournamentGroup group)
    {
        return new HockeyTournamentGroupDto(
            group.Id,
            group.TournamentId,
            group.Name,
            group.SortOrder,
            group.Teams.Select(ToGroupTeamDto).ToList());
    }

    /// <summary>
    /// Maps a tournament group-team membership to a DTO.
    /// </summary>
    public static HockeyTournamentGroupTeamDto ToGroupTeamDto(HockeyTournamentGroupTeam groupTeam)
    {
        return new HockeyTournamentGroupTeamDto(
            groupTeam.Id,
            groupTeam.TournamentGroupId,
            groupTeam.CompetitionTeamId,
            groupTeam.Seed,
            groupTeam.IsActive);
    }

    /// <summary>
    /// Maps a playoff series to a DTO.
    /// </summary>
    public static HockeyPlayoffSeriesDto ToPlayoffSeriesDto(
        HockeyPlayoffSeries series,
        IEnumerable<HockeyMatch>? matches = null)
    {
        (int homeWins, int awayWins) = CountSeriesWins(series, matches);
        return new HockeyPlayoffSeriesDto(
            series.Id,
            series.CompetitionId,
            series.Round.ToString(),
            series.SeriesOrder,
            series.BestOf,
            series.HomeCompetitionTeamId,
            series.AwayCompetitionTeamId,
            homeWins,
            awayWins,
            series.WinnerCompetitionTeamId,
            series.Status.ToString());
    }

    private static (int HomeWins, int AwayWins) CountSeriesWins(
        HockeyPlayoffSeries series,
        IEnumerable<HockeyMatch>? matches)
    {
        if (matches is null)
            return (series.HomeTeamWins, series.AwayTeamWins);

        int homeWins = 0;
        int awayWins = 0;
        foreach (HockeyMatch match in matches.Where(item =>
            item.PlayoffSeriesId == series.Id
            && item.Status == HockeyMatchStatus.Finished
            && item.HomeScore != item.AwayScore))
        {
            Guid? winnerCompetitionTeamId = match.HomeScore > match.AwayScore
                ? match.HomeMatchTeam?.CompetitionTeamId
                : match.AwayMatchTeam?.CompetitionTeamId;
            if (winnerCompetitionTeamId == series.HomeCompetitionTeamId)
                homeWins++;
            else if (winnerCompetitionTeamId == series.AwayCompetitionTeamId)
                awayWins++;
        }

        return (homeWins, awayWins);
    }

    /// <summary>
    /// Maps tournament rules to a DTO.
    /// </summary>
    public static HockeyTournamentRulesDto ToTournamentRulesDto(HockeyTournamentRules rules)
    {
        return new HockeyTournamentRulesDto(
            rules.Format.ToString(),
            rules.HasGroupStage,
            rules.HasPlayoffs,
            rules.HasBronzeGame,
            rules.HasPlacementGames,
            rules.TeamsAdvancingPerGroup);
    }

    /// <summary>
    /// Maps a playoff schedule slot to a DTO.
    /// </summary>
    public static HockeyPlayoffScheduleSlotDto ToPlayoffScheduleSlotDto(HockeyPlayoffScheduleSlot slot)
    {
        return new HockeyPlayoffScheduleSlotDto(
            slot.Round,
            slot.SeriesOrder,
            slot.MatchOrder,
            slot.HomeSourceType,
            slot.AwaySourceType,
            slot.HomeSourceGroupId,
            slot.AwaySourceGroupId,
            slot.HomeSourceSeriesId,
            slot.AwaySourceSeriesId,
            slot.HomeSourceRank,
            slot.AwaySourceRank,
            slot.ManualHomeCompetitionTeamId,
            slot.ManualAwayCompetitionTeamId);
    }
}
