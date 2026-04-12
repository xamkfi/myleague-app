using Application.Features.Floorball.Matches.DTOs;

namespace Application.Features.Floorball.Tournaments.DTOs;

/// <summary>
/// Data Transfer Object for FloorballTournament entity
/// </summary>
/// <param name="Id">The unique identifier of the tournament</param>
/// <param name="Name">The name of the tournament</param>
/// <param name="StartDate">The start date of the tournament</param>
/// <param name="EndDate">The end date of the tournament</param>
/// <param name="IsActive">Whether the tournament is currently active</param>
/// <param name="IsCompleted">Whether the tournament is completed</param>
/// <param name="ContentHtml">Optional HTML content describing the tournament</param>
/// <param name="Venue">Optional primary venue for the tournament</param>
/// <param name="TournamentStatus">String representation of the tournament lifecycle status</param>
/// <param name="TournamentRules">Tournament-specific rules configuration</param>
/// <param name="Groups">List of groups within this tournament</param>
/// <param name="TeamCount">Total number of teams across all groups</param>
/// <param name="MatchCount">Total number of matches in the tournament</param>
public record FloorballTournamentDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsCompleted,
    string? ContentHtml,
    string? Venue,
    string TournamentStatus,
    FloorballTournamentRulesDto TournamentRules,
    List<FloorballTournamentGroupDto> Groups,
    int TeamCount,
    int MatchCount);
