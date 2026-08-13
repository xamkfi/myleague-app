using System;
using System.Collections.Generic;
using Domain.Enums.Football;

namespace Application.Features.Football.Players.DTOs
{
    /// <summary>
    /// Data Transfer Object for a football player with their match history and performance statistics
    /// </summary>
    public record FootballPlayerWithMatchesDto(
        Guid Id,
        string PlayerName,
        FootballPosition Position,
        int? JerseyNumber,
        string TeamName,
        Guid TeamId,
        bool IsActive,
        IReadOnlyCollection<FootballPlayerTeamCareerStatsDto> CareerStats,
        IReadOnlyCollection<FootballPlayerMatchDto> RecentMatches);

    /// <summary>
    /// Data Transfer Object for a player's career statistics with a specific team
    /// </summary>
    public record FootballPlayerTeamCareerStatsDto(
        Guid TeamId,
        string TeamName,
        FootballPlayerStatsDto Stats);

    /// <summary>
    /// Data Transfer Object for player statistics
    /// </summary>
    public record FootballPlayerStatsDto(
        int GamesPlayed,
        int Goals,
        int Assists,
        int Points,
        int YellowCards,
        int RedCards);

    /// <summary>
    /// Data Transfer Object for a player's performance in a specific match
    /// </summary>
    public record FootballPlayerMatchDto(
        Guid Id,
        Guid CompetitionId,
        string CompetitionName,
        Guid? HomeTeamId,
        string HomeTeamName,
        Guid? AwayTeamId,
        string AwayTeamName,
        DateTime ScheduledDateTime,
        string? Venue,
        FootballMatchStatus Status,
        int HomeScore,
        int AwayScore,
        bool WentToExtraTime,
        bool WentToPenaltyShootout,
        IReadOnlyDictionary<int, (int HomeScore, int AwayScore)> PeriodScores,
        FootballPlayerMatchStatsDto? PlayerStats);

    /// <summary>
    /// Data Transfer Object for a player's statistics in a specific match
    /// </summary>
    public record FootballPlayerMatchStatsDto(
        int Goals,
        int Assists,
        int YellowCards,
        int RedCards,
        int PlayedMinutes);
} 
