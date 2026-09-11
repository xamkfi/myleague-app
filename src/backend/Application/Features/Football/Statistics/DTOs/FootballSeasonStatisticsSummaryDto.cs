using Application.Features.Football.Teams.DTOs;

namespace Application.Features.Football.Statistics.DTOs;

/// <summary>
/// DTO for comprehensive football season statistics summary
/// </summary>
public class FootballSeasonStatisticsSummaryDto
{
    public Guid CompetitionId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public List<FootballTeamSeasonStatisticsDto> TeamStandings { get; set; } = new();
    public List<FootballPlayerSeasonStatisticsDto> TopScorers { get; set; } = new();
    public List<FootballPlayerSeasonStatisticsDto> TopAssists { get; set; } = new();
    public int TotalGames { get; set; }
    public int TotalGoals { get; set; }
    public decimal AverageGoalsPerGame { get; set; }
}
