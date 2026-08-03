using Application.Features.Hockey.Competitions.DTOs;

namespace Application.Features.Hockey.Seasons.DTOs;

/// <summary>
/// Data transfer object for a hockey season.
/// </summary>
/// <param name="Id">Unique identifier of the season</param>
/// <param name="Name">Display name of the season</param>
/// <param name="StartDate">Season start date</param>
/// <param name="EndDate">Season end date</param>
/// <param name="Status">Lifecycle status as a string</param>
/// <param name="IsActive">Whether the season is currently active</param>
/// <param name="IsCompleted">Whether the season is completed</param>
/// <param name="SeasonCode">Optional short season code (e.g. 2026-27)</param>
/// <param name="ChampionCompetitionTeamId">Champion competition-team id when set</param>
/// <param name="Teams">Teams registered in this season</param>
public record HockeySeasonDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    bool IsActive,
    bool IsCompleted,
    string? SeasonCode,
    Guid? ChampionCompetitionTeamId,
    IReadOnlyCollection<HockeyCompetitionTeamDto> Teams);
