using Domain.Enums.Common;

namespace Application.Features.Football.Seasons.DTOs;

/// <summary>
/// Slim season DTO for public listing (no teams/matches/rules payload).
/// </summary>
public record FootballSeasonSummaryDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsCompleted,
    string SeasonYear,
    TeamCategory TeamCategory);
