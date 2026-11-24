namespace Application.Features.Football.Statistics.DTOs;

/// <summary>
/// DTO for football per-match team statistics
/// </summary>
public class FootballMatchTeamStatisticsDto
{
    public Guid MatchId { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int Goals { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public bool CleanSheet { get; set; }
}
