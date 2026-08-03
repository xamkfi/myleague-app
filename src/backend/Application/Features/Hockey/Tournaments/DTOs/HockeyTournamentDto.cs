using Application.Features.Hockey.Competitions.DTOs;

namespace Application.Features.Hockey.Tournaments.DTOs;

/// <summary>
/// Data transfer object for a hockey tournament.
/// </summary>
/// <param name="Id">Unique identifier of the tournament</param>
/// <param name="Name">Display name of the tournament</param>
/// <param name="StartDate">Tournament start date</param>
/// <param name="EndDate">Tournament end date</param>
/// <param name="Status">Lifecycle status as a string</param>
/// <param name="IsActive">Whether the tournament is currently active</param>
/// <param name="IsCompleted">Whether the tournament is completed</param>
/// <param name="Venue">Optional primary venue</param>
/// <param name="ContentHtml">Optional HTML description</param>
/// <param name="CurrentStage">Current tournament stage as a string</param>
/// <param name="ChampionCompetitionTeamId">Champion competition-team id when set</param>
/// <param name="Teams">Teams registered in this tournament</param>
public record HockeyTournamentDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    bool IsActive,
    bool IsCompleted,
    string? Venue,
    string? ContentHtml,
    string CurrentStage,
    Guid? ChampionCompetitionTeamId,
    IReadOnlyCollection<HockeyCompetitionTeamDto> Teams);
