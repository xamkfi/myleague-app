using Application.Features.Football.Matches.DTOs;
using Domain.Enums.Common;

namespace Application.Features.Football.Seasons.DTOs;

/// <summary>
/// Data Transfer Object for FootballSeason entity.
/// </summary>
public record FootballSeasonDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsCompleted,
    IReadOnlyCollection<FootballSeasonDivisionDto> SeasonDivisions,
    IReadOnlyCollection<FootballTeamSummaryDto> Teams,
    IReadOnlyCollection<FootballMatchDto> Matches,
    FootballMatchRulesDto MatchRules,
    FootballStandingRulesDto StandingRules,
    TeamCategory TeamCategory);
