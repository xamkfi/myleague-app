using Application.Features.Hockey.Competitions.DTOs;

namespace Application.Features.Hockey.Seasons.DTOs;

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
