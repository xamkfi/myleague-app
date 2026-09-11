using System;

namespace Application.Features.Floorball.Statistics.DTOs;

/// <summary>
/// DTO for floorball goalie season statistics
/// </summary>
public class FloorballGoalieSeasonStatisticsDto
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the player ID
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the team ID
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Gets or sets the season ID
    /// </summary>
    public Guid CompetitionId { get; set; }

    /// <summary>
    /// Gets or sets the player name
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the team name
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the season name
    /// </summary>
    public string SeasonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of games played
    /// </summary>
    public int GamesPlayed { get; set; }

    /// <summary>
    /// Gets or sets the number of games started
    /// </summary>
    public int GamesStarted { get; set; }

    /// <summary>
    /// Gets or sets the number of wins
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Gets or sets the number of losses
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Gets or sets the number of ties
    /// </summary>
    public int Ties { get; set; }

    /// <summary>
    /// Gets or sets the number of saves made
    /// </summary>
    public int Saves { get; set; }

    /// <summary>
    /// Gets or sets the number of shots faced
    /// </summary>
    public int ShotsAgainst { get; set; }

    /// <summary>
    /// Gets or sets the save percentage
    /// </summary>
    public decimal SavePercentage { get; set; }

    /// <summary>
    /// Gets or sets the number of goals allowed
    /// </summary>
    public int GoalsAgainst { get; set; }

    /// <summary>
    /// Gets or sets the goals against average
    /// </summary>
    public decimal GoalsAgainstAverage { get; set; }

    /// <summary>
    /// Gets or sets the number of shutouts
    /// </summary>
    public int Shutouts { get; set; }

    /// <summary>
    /// Gets or sets the number of minutes played
    /// </summary>
    public int MinutesPlayed { get; set; }

    /// <summary>
    /// Gets or sets the number of power play saves
    /// </summary>
    public int PowerPlaySaves { get; set; }

    /// <summary>
    /// Gets or sets the number of power play shots faced
    /// </summary>
    public int PowerPlayShotsAgainst { get; set; }

    /// <summary>
    /// Gets or sets the power play save percentage
    /// </summary>
    public decimal PowerPlaySavePercentage { get; set; }

    /// <summary>
    /// Gets or sets the number of short-handed saves
    /// </summary>
    public int ShortHandedSaves { get; set; }

    /// <summary>
    /// Gets or sets the number of short-handed shots faced
    /// </summary>
    public int ShortHandedShotsAgainst { get; set; }

    /// <summary>
    /// Gets or sets the short-handed save percentage
    /// </summary>
    public decimal ShortHandedSavePercentage { get; set; }
}
