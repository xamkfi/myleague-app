using System;
using System.Collections.Generic;
using Domain.Enums.Floorball;

namespace Application.Features.Floorball.Players.DTOs
{
    /// <summary>
    /// Data Transfer Object for a floorball player with their match history and performance statistics
    /// </summary>
    public record FloorballPlayerWithMatchesDto(
        Guid Id,
        string PlayerName,
        FloorballPosition Position,
        int? JerseyNumber,
        string TeamName,
        Guid TeamId,
        bool IsActive,
        IReadOnlyCollection<FloorballPlayerTeamCareerStatsDto> CareerStats,
        IReadOnlyCollection<FloorballPlayerMatchDto> RecentMatches);

    /// <summary>
    /// Data Transfer Object for a player's career statistics with a specific team
    /// </summary>
    public record FloorballPlayerTeamCareerStatsDto(
        Guid TeamId,
        string TeamName,
        FloorballPlayerStatsDto Stats);

    /// <summary>
    /// Data Transfer Object for player statistics
    /// </summary>
    public record FloorballPlayerStatsDto(
        int GamesPlayed,
        int Goals,
        int Assists,
        int Points,
        int PenaltyMinutes);

    /// <summary>
    /// Data Transfer Object for a player's performance in a specific match
    /// </summary>
    public record FloorballPlayerMatchDto(
        Guid Id,
        Guid SeasonId,
        Guid HomeTeamId,
        string HomeTeamName,
        Guid AwayTeamId,
        string AwayTeamName,
        DateTime ScheduledDateTime,
        string? Venue,
        FloorballMatchStatus Status,
        int HomeScore,
        int AwayScore,
        bool WentToOvertime,
        bool WentToShootout,
        IReadOnlyDictionary<int, (int HomeScore, int AwayScore)> PeriodScores,
        FloorballPlayerMatchStatsDto? PlayerStats);

    /// <summary>
    /// Data Transfer Object for a player's statistics in a specific match
    /// </summary>
    public record FloorballPlayerMatchStatsDto(
        int Goals,
        int Assists,
        int PenaltyMinutes,
        int PlayedMinutes);
} 
