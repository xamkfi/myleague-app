namespace Application.Features.Floorball.Seasons.DTOs;

/// <summary>
/// Slim season DTO for public listing (no teams/matches/rules payload).
/// </summary>
public record FloorballSeasonSummaryDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsCompleted,
    string SeasonYear);
