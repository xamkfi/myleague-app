using Domain.Entities;
using System.Globalization;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents comprehensive team statistics for a specific season
/// </summary>
public class FloorballTeamSeasonStatistics : BaseEntity
{
    /// <summary>
    /// Gets the ID of the team these statistics belong to
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the team these statistics belong to
    /// </summary>
    public FloorballTeam Team { get; private set; }

    /// <summary>
    /// Gets the ID of the season these statistics are for
    /// </summary>
    public Guid SeasonId { get; private set; }

    /// <summary>
    /// Gets the season these statistics are for
    /// </summary>
    public FloorballSeason Season { get; private set; }

    // Basic game statistics
    /// <summary>
    /// Gets the number of games played
    /// </summary>
    public int GamesPlayed { get; private set; }

    /// <summary>
    /// Gets the number of wins
    /// </summary>
    public int Wins { get; private set; }

    /// <summary>
    /// Gets the number of losses
    /// </summary>
    public int Losses { get; private set; }

    /// <summary>
    /// Gets the number of ties/overtime losses
    /// </summary>
    public int Ties { get; private set; }

    /// <summary>
    /// Gets the total points earned (wins * 3 + ties * 1)
    /// </summary>
    public int Points { get; private set; }

    // Scoring statistics
    /// <summary>
    /// Gets the total goals scored by the team
    /// </summary>
    public int GoalsFor { get; private set; }

    /// <summary>
    /// Gets the total goals conceded by the team
    /// </summary>
    public int GoalsAgainst { get; private set; }

    /// <summary>
    /// Gets the goal difference (goals for - goals against)
    /// </summary>
    public int GoalDifference { get; private set; }

    // Shot statistics
    /// <summary>
    /// Gets the total shots taken by the team
    /// </summary>
    public int ShotsFor { get; private set; }

    /// <summary>
    /// Gets the total shots faced by the team
    /// </summary>
    public int ShotsAgainst { get; private set; }

    /// <summary>
    /// Gets the team's shot percentage
    /// </summary>
    public decimal ShotPercentage { get; private set; }

    // Power play statistics
    /// <summary>
    /// Gets the number of power play goals scored
    /// </summary>
    public int PowerPlayGoals { get; private set; }

    /// <summary>
    /// Gets the number of power play opportunities
    /// </summary>
    public int PowerPlayOpportunities { get; private set; }

    /// <summary>
    /// Gets the power play success percentage
    /// </summary>
    public decimal PowerPlayPercentage { get; private set; }

    // Penalty kill statistics
    /// <summary>
    /// Gets the number of short-handed goals scored
    /// </summary>
    public int ShortHandedGoals { get; private set; }

    /// <summary>
    /// Gets the number of penalty kill opportunities
    /// </summary>
    public int PenaltyKillOpportunities { get; private set; }

    /// <summary>
    /// Gets the penalty kill success percentage
    /// </summary>
    public decimal PenaltyKillPercentage { get; private set; }

    // Penalty statistics
    /// <summary>
    /// Gets the total penalty minutes
    /// </summary>
    public int PenaltyMinutes { get; private set; }

    // Faceoff statistics
    /// <summary>
    /// Gets the number of faceoffs won
    /// </summary>
    public int FaceoffWins { get; private set; }

    /// <summary>
    /// Gets the total number of faceoffs taken
    /// </summary>
    public int FaceoffAttempts { get; private set; }

    /// <summary>
    /// Gets the faceoff win percentage
    /// </summary>
    public decimal FaceoffPercentage { get; private set; }

    // Home/Away statistics
    /// <summary>
    /// Gets the number of home wins
    /// </summary>
    public int HomeWins { get; private set; }

    /// <summary>
    /// Gets the number of home losses
    /// </summary>
    public int HomeLosses { get; private set; }

    /// <summary>
    /// Gets the number of away wins
    /// </summary>
    public int AwayWins { get; private set; }

    /// <summary>
    /// Gets the number of away losses
    /// </summary>
    public int AwayLosses { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballTeamSeasonStatistics()
    {
        Team = null!;
        Season = null!;
    }

    /// <summary>
    /// Initializes a new instance of FloorballTeamSeasonStatistics
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="seasonId">The season ID</param>
    public FloorballTeamSeasonStatistics(Guid teamId, Guid seasonId)
    {
        TeamId = teamId;
        SeasonId = seasonId;
        Team = null!;
        Season = null!;
        
        // Initialize all statistics to zero
        GamesPlayed = 0;
        Wins = 0;
        Losses = 0;
        Ties = 0;
        Points = 0;
        GoalsFor = 0;
        GoalsAgainst = 0;
        GoalDifference = 0;
        ShotsFor = 0;
        ShotsAgainst = 0;
        ShotPercentage = 0;
        PowerPlayGoals = 0;
        PowerPlayOpportunities = 0;
        PowerPlayPercentage = 0;
        ShortHandedGoals = 0;
        PenaltyKillOpportunities = 0;
        PenaltyKillPercentage = 0;
        PenaltyMinutes = 0;
        FaceoffWins = 0;
        FaceoffAttempts = 0;
        FaceoffPercentage = 0;
        HomeWins = 0;
        HomeLosses = 0;
        AwayWins = 0;
        AwayLosses = 0;
    }

    /// <summary>
    /// Updates the team's statistics after a match
    /// </summary>
    /// <param name="gameResult">Result of the game (win/loss/tie)</param>
    /// <param name="isHomeGame">Whether this was a home game</param>
    /// <param name="goalsFor">Goals scored by the team</param>
    /// <param name="goalsAgainst">Goals scored against the team</param>
    /// <param name="shotsFor">Shots taken by the team</param>
    /// <param name="shotsAgainst">Shots faced by the team</param>
    /// <param name="powerPlayGoals">Power play goals scored</param>
    /// <param name="powerPlayOpportunities">Power play opportunities</param>
    /// <param name="shortHandedGoals">Short-handed goals scored</param>
    /// <param name="penaltyKillOpportunities">Penalty kill opportunities</param>
    /// <param name="penaltyMinutes">Penalty minutes taken</param>
    /// <param name="faceoffWins">Faceoffs won</param>
    /// <param name="faceoffAttempts">Total faceoffs taken</param>
    public void UpdateAfterMatch(
        string gameResult,
        bool isHomeGame,
        int goalsFor,
        int goalsAgainst,
        int shotsFor = 0,
        int shotsAgainst = 0,
        int powerPlayGoals = 0,
        int powerPlayOpportunities = 0,
        int shortHandedGoals = 0,
        int penaltyKillOpportunities = 0,
        int penaltyMinutes = 0,
        int faceoffWins = 0,
        int faceoffAttempts = 0)
    {
        GamesPlayed++;
        
        // Validate parameters
        if (string.IsNullOrWhiteSpace(gameResult))
            throw new ArgumentNullException(nameof(gameResult), "Game result cannot be null or empty.");
            
        if (goalsFor < 0 || goalsAgainst < 0)
            throw new ArgumentException("Goals cannot be negative.");

        // Update win/loss/tie record
        switch (gameResult.ToUpperInvariant())
        {
            case "WIN":
                Wins++;
                Points += 3;
                if (isHomeGame) HomeWins++;
                else AwayWins++;
                break;
            case "LOSS":
                Losses++;
                if (isHomeGame) HomeLosses++;
                else AwayLosses++;
                break;
            case "TIE":
                Ties++;
                Points += 1;
                break;
        }

        // Update scoring statistics
        GoalsFor += goalsFor;
        GoalsAgainst += goalsAgainst;
        GoalDifference = GoalsFor - GoalsAgainst;

        // Update shot statistics
        ShotsFor += shotsFor;
        ShotsAgainst += shotsAgainst;
        ShotPercentage = ShotsFor > 0 ? (decimal)GoalsFor / ShotsFor * 100 : 0;

        // Update special teams statistics
        PowerPlayGoals += powerPlayGoals;
        PowerPlayOpportunities += powerPlayOpportunities;
        PowerPlayPercentage = PowerPlayOpportunities > 0 ? (decimal)PowerPlayGoals / PowerPlayOpportunities * 100 : 0;

        ShortHandedGoals += shortHandedGoals;
        PenaltyKillOpportunities += penaltyKillOpportunities;
        PenaltyKillPercentage = PenaltyKillOpportunities > 0 ? (decimal)(PenaltyKillOpportunities - (GoalsAgainst - PowerPlayGoals)) / PenaltyKillOpportunities * 100 : 0;

        // Update penalty statistics
        PenaltyMinutes += penaltyMinutes;

        // Update faceoff statistics
        FaceoffWins += faceoffWins;
        FaceoffAttempts += faceoffAttempts;
        FaceoffPercentage = FaceoffAttempts > 0 ? (decimal)FaceoffWins / FaceoffAttempts * 100 : 0;
    }
}
