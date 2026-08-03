using Application.Features.Hockey.Competitions.DTOs;

namespace Application.Features.Hockey.Tournaments.DTOs;

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
