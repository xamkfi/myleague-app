namespace Application.Features.Hockey.Statistics.DTOs;

/// <summary>
/// One player's hockey totals across the selected competitions.
/// </summary>
public record HockeyAllTimePlayerStatisticsDto(
    int Rank,
    Guid PlayerId,
    string PlayerName,
    string TeamName,
    int GamesPlayed,
    int Goals,
    int Assists,
    int Points,
    int PenaltyMinutes);
