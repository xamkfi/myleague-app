using Application.Features.Hockey.Seasons.DTOs;
using Application.Features.Hockey.Tournaments.DTOs;

namespace Application.Features.Hockey.Competitions.DTOs;

/// <summary>
/// Shared summary DTO for a hockey competition (season or tournament).
/// </summary>
public record HockeyCompetitionDto(
    Guid Id,
    string Name,
    string CompetitionType,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    bool IsActive,
    bool IsCompleted,
    IReadOnlyCollection<HockeyCompetitionTeamDto> Teams,
    IReadOnlyCollection<HockeyCompetitionDivisionDto> Divisions,
    IReadOnlyCollection<HockeyPlayoffSeriesDto> PlayoffSeries);
