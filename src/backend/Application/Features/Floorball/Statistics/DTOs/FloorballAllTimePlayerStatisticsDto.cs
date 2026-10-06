namespace Application.Features.Floorball.Statistics.DTOs;

/// <summary>
/// One player's floorball totals across the selected competitions.
/// </summary>
public record FloorballAllTimePlayerStatisticsDto(
    int Rank,
    Guid PlayerId,
    string PlayerName,
    string TeamName,
    int GamesPlayed,
    int Goals,
    int Assists,
    int Points,
    int PenaltyMinutes);
