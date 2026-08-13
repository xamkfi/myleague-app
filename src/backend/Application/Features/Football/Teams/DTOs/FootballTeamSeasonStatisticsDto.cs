using Domain.Enums.Football;

namespace Application.Features.Football.Teams.DTOs;

/// <summary>
/// DTO for football team season statistics
/// </summary>
public class FootballTeamSeasonStatisticsDto
{
    /// <summary>
    /// Gets or sets the statistics ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the team ID
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Gets or sets the competition ID
    /// </summary>
    public Guid CompetitionId { get; set; }

    /// <summary>
    /// Gets or sets team name
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets team logo
    /// </summary>
    public Uri? TeamLogo { get; set; }

    /// <summary>
    /// Gets or sets season name
    /// </summary>
    public string SeasonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of games played
    /// </summary>
    public int GamesPlayed { get; set; }

    /// <summary>
    /// Gets or sets the number of wins
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Gets or sets the number of losses
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Gets or sets the number of draws
    /// </summary>
    public int Draws { get; set; }

    /// <summary>
    /// Gets or sets the total points
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets goals scored
    /// </summary>
    public int GoalsFor { get; set; }

    /// <summary>
    /// Gets or sets goals conceded
    /// </summary>
    public int GoalsAgainst { get; set; }

    /// <summary>
    /// Gets or sets goal difference
    /// </summary>
    public int GoalDifference { get; set; }

    /// <summary>
    /// Gets or sets home wins
    /// </summary>
    public int HomeWins { get; set; }

    /// <summary>
    /// Gets or sets home losses
    /// </summary>
    public int HomeLosses { get; set; }

    /// <summary>
    /// Gets or sets away wins
    /// </summary>
    public int AwayWins { get; set; }

    /// <summary>
    /// Gets or sets away losses
    /// </summary>
    public int AwayLosses { get; set; }

    /// <summary>
    /// Gets or sets clean sheets
    /// </summary>
    public int CleanSheets { get; set; }

    /// <summary>
    /// Gets or sets yellow cards
    /// </summary>
    public int YellowCards { get; set; }

    /// <summary>
    /// Gets or sets red cards
    /// </summary>
    public int RedCards { get; set; }

    /// <summary>
    /// Gets or sets last five form. values are W, L, D
    /// </summary>
    public FootballGameResult[] LastFiveForm { get; set; } = Array.Empty<FootballGameResult>();
}
