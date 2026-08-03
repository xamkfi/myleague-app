using Domain.Enums.Hockey.Competitions;

namespace Application.Features.Hockey.Tournaments.DTOs;

/// <summary>
/// Summary of hockey tournament rules.
/// </summary>
public record HockeyTournamentRulesDto(
    string Format,
    bool HasGroupStage,
    bool HasPlayoffs,
    bool HasBronzeGame,
    bool HasPlacementGames,
    int TeamsAdvancingPerGroup);

/// <summary>
/// Data transfer object for a hockey playoff series.
/// </summary>
public record HockeyPlayoffSeriesDto(
    Guid Id,
    Guid CompetitionId,
    string Round,
    int SeriesOrder,
    int BestOf,
    Guid? HomeCompetitionTeamId,
    Guid? AwayCompetitionTeamId,
    int HomeTeamWins,
    int AwayTeamWins,
    Guid? WinnerCompetitionTeamId,
    string Status);

/// <summary>
/// Input for one playoff schedule slot.
/// </summary>
public record HockeyPlayoffScheduleSlotDto(
    HockeyPlayoffRound Round,
    int SeriesOrder,
    int MatchOrder,
    HockeyPlayoffSourceType HomeSourceType,
    HockeyPlayoffSourceType AwaySourceType,
    Guid? HomeSourceGroupId = null,
    Guid? AwaySourceGroupId = null,
    Guid? HomeSourceSeriesId = null,
    Guid? AwaySourceSeriesId = null,
    int? HomeSourceRank = null,
    int? AwaySourceRank = null,
    Guid? ManualHomeCompetitionTeamId = null,
    Guid? ManualAwayCompetitionTeamId = null);
