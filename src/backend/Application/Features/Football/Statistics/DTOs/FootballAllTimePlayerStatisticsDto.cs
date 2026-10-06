namespace Application.Features.Football.Statistics.DTOs;

/// <summary>
/// One player's football totals across the selected competitions.
/// </summary>
public record FootballAllTimePlayerStatisticsDto(
    int Rank,
    Guid PlayerId,
    string PlayerName,
    string TeamName,
    int GamesPlayed,
    int Goals,
    int Assists,
    int Points,
    int YellowCards,
    int RedCards);
