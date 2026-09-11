namespace Application.Features.Football.Statistics.DTOs;

/// <summary>
/// DTO for football player season statistics
/// </summary>
public class FootballPlayerSeasonStatisticsDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public Guid CompetitionId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string? TeamLogo { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Points { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
}
