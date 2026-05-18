using Domain.Entities;
using Domain.Enums.Floorball;
using System.Globalization;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents comprehensive goalie statistics for a specific season
/// </summary>
public class FloorballGoalieSeasonStatistics : BaseEntity
{
    /// <summary>
    /// Gets the ID of the player (goalie) these statistics belong to
    /// </summary>
    public Guid PlayerId { get; private set; }

    /// <summary>
    /// Gets the player (goalie) these statistics belong to
    /// </summary>
    public FloorballPlayer Player { get; private set; }

    /// <summary>
    /// Gets the ID of the team the goalie played for during this season
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the team the goalie played for during this season
    /// </summary>
    public FloorballTeam Team { get; private set; }

    /// <summary>
    /// Gets the ID of the competition these statistics are for
    /// </summary>
    public Guid CompetitionId { get; private set; }

    /// <summary>
    /// Gets the competition these statistics are for
    /// </summary>
    public FloorballCompetition Competition { get; private set; }

    // Basic goalie statistics
    /// <summary>
    /// Gets the number of games played
    /// </summary>
    public int GamesPlayed { get; private set; }

    /// <summary>
    /// Gets the number of games started as the primary goalie
    /// </summary>
    public int GamesStarted { get; private set; }

    /// <summary>
    /// Gets the number of wins
    /// </summary>
    public int Wins { get; private set; }

    /// <summary>
    /// Gets the number of losses
    /// </summary>
    public int Losses { get; private set; }

    /// <summary>
    /// Gets the number of ties
    /// </summary>
    public int Ties { get; private set; }

    // Save statistics
    /// <summary>
    /// Gets the total number of saves made
    /// </summary>
    public int Saves { get; private set; }

    /// <summary>
    /// Gets the total number of shots faced
    /// </summary>
    public int ShotsAgainst { get; private set; }

    /// <summary>
    /// Gets the save percentage
    /// </summary>
    public decimal SavePercentage { get; private set; }

    /// <summary>
    /// Gets the total goals allowed
    /// </summary>
    public int GoalsAgainst { get; private set; }

    /// <summary>
    /// Gets the goals against average (GAA)
    /// </summary>
    public decimal GoalsAgainstAverage { get; private set; }

    /// <summary>
    /// Gets the number of shutouts, aka nollapeli
    /// </summary>
    public int Shutouts { get; private set; }

    /// <summary>
    /// Gets the total minutes played
    /// </summary>
    public int MinutesPlayed { get; private set; }

    // Power play statistics
    /// <summary>
    /// Gets the number of power play saves
    /// </summary>
    public int PowerPlaySaves { get; private set; }

    /// <summary>
    /// Gets the number of power play shots faced
    /// </summary>
    public int PowerPlayShotsAgainst { get; private set; }

    /// <summary>
    /// Gets the power play save percentage
    /// </summary>
    public decimal PowerPlaySavePercentage { get; private set; }

    // Short-handed statistics
    /// <summary>
    /// Gets the number of short-handed saves
    /// </summary>
    public int ShortHandedSaves { get; private set; }

    /// <summary>
    /// Gets the number of short-handed shots faced
    /// </summary>
    public int ShortHandedShotsAgainst { get; private set; }

    /// <summary>
    /// Gets the short-handed save percentage
    /// </summary>
    public decimal ShortHandedSavePercentage { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballGoalieSeasonStatistics()
    {
        Player = null!;
        Team = null!;
        Competition = null!;
    }

    /// <summary>
    /// Initializes a new instance of FloorballGoalieSeasonStatistics
    /// </summary>
    /// <param name="playerId">The player (goalie) ID</param>
    /// <param name="teamId">The team ID</param>
    /// <param name="competitionId">The competition ID</param>
    public FloorballGoalieSeasonStatistics(Guid playerId, Guid teamId, Guid competitionId)
    {
        PlayerId = playerId;
        TeamId = teamId;
        CompetitionId = competitionId;
        Player = null!;
        Team = null!;
        Competition = null!;
        
        // Initialize all statistics to zero
        GamesPlayed = 0;
        GamesStarted = 0;
        Wins = 0;
        Losses = 0;
        Ties = 0;
        Saves = 0;
        ShotsAgainst = 0;
        SavePercentage = 0;
        GoalsAgainst = 0;
        GoalsAgainstAverage = 0;
        Shutouts = 0;
        MinutesPlayed = 0;
        PowerPlaySaves = 0;
        PowerPlayShotsAgainst = 0;
        PowerPlaySavePercentage = 0;
        ShortHandedSaves = 0;
        ShortHandedShotsAgainst = 0;
        ShortHandedSavePercentage = 0;
    }

    /// <summary>
    /// Records a game played by this goalie
    /// </summary>
    /// <param name="wasStarter">Whether the goalie started the game</param>
    /// <param name="gameResult">Result of the game</param>
    /// <param name="minutesPlayed">Minutes played in the game</param>
    /// <param name="wasShutout">Whether the goalie kept a clean sheet (no goals allowed)</param>
    public void RecordGamePlayed(bool wasStarter, FloorballGameResult gameResult, int minutesPlayed, bool wasShutout = false)
    {
        if (minutesPlayed < 0)
            throw new ArgumentException("Minutes played cannot be negative.", nameof(minutesPlayed));

        GamesPlayed++;
        if (wasStarter) GamesStarted++;
        
        switch (gameResult)
        {
            case FloorballGameResult.Win:
                Wins++;
                break;
            case FloorballGameResult.Loss:
                Losses++;
                break;
            case FloorballGameResult.Tie:
                Ties++;
                break;
            default:
                throw new ArgumentException($"Invalid game result: {gameResult}", nameof(gameResult));
        }

        if (wasShutout) Shutouts++;

        MinutesPlayed += minutesPlayed;
        UpdateGoalsAgainstAverage();
    }

    /// <summary>
    /// Records saves and shots faced by this goalie
    /// </summary>
    /// <param name="saves">Number of saves made (can be negative for decrementing)</param>
    /// <param name="shotsAgainst">Number of shots faced (can be negative for decrementing)</param>
    /// <param name="goalsAllowed">Number of goals allowed (can be negative for decrementing)</param>
    public void RecordSaves(int saves, int shotsAgainst, int goalsAllowed)
    {
        // Allow negative values for decrementing statistics
        if (Math.Abs(saves) + Math.Abs(goalsAllowed) != Math.Abs(shotsAgainst) && shotsAgainst != 0)
            throw new ArgumentException("Absolute values: |saves| + |goals allowed| must equal |shots against|.");

        Saves += saves;
        ShotsAgainst += shotsAgainst;
        GoalsAgainst += goalsAllowed;

        Saves = Math.Max(0, Saves);
        ShotsAgainst = Math.Max(0, ShotsAgainst);
        GoalsAgainst = Math.Max(0, GoalsAgainst);

        UpdateSavePercentage();
        UpdateGoalsAgainstAverage();
    }

    /// <summary>
    /// Records power play saves and shots
    /// </summary>
    /// <param name="saves">Number of power play saves</param>
    /// <param name="shotsAgainst">Number of power play shots faced</param>
    public void RecordPowerPlaySaves(int saves, int shotsAgainst)
    {
        if (saves < 0 || shotsAgainst < 0 || saves > shotsAgainst)
            throw new ArgumentException("Invalid power play save statistics.");

        PowerPlaySaves += saves;
        PowerPlayShotsAgainst += shotsAgainst;
        UpdatePowerPlaySavePercentage();
    }

    /// <summary>
    /// Records short-handed saves and shots
    /// </summary>
    /// <param name="saves">Number of short-handed saves</param>
    /// <param name="shotsAgainst">Number of short-handed shots faced</param>
    public void RecordShortHandedSaves(int saves, int shotsAgainst)
    {
        if (saves < 0 || shotsAgainst < 0 || saves > shotsAgainst)
            throw new ArgumentException("Invalid short-handed save statistics.");

        ShortHandedSaves += saves;
        ShortHandedShotsAgainst += shotsAgainst;
        UpdateShortHandedSavePercentage();
    }

    /// <summary>
    /// Updates the save percentage based on current saves and shots
    /// </summary>
    private void UpdateSavePercentage()
    {
        SavePercentage = ShotsAgainst > 0 ? (decimal)Saves / ShotsAgainst * 100 : 0;
    }

    /// <summary>
    /// Updates the goals against average based on current goals and minutes
    /// </summary>
    private void UpdateGoalsAgainstAverage()
    {
        // GAA = (Goals Against × 60) / Minutes Played
        GoalsAgainstAverage = MinutesPlayed > 0 ? (decimal)GoalsAgainst * 60 / MinutesPlayed : 0;
    }

    /// <summary>
    /// Updates the power play save percentage
    /// </summary>
    private void UpdatePowerPlaySavePercentage()
    {
        PowerPlaySavePercentage = PowerPlayShotsAgainst > 0 ? (decimal)PowerPlaySaves / PowerPlayShotsAgainst * 100 : 0;
    }

    /// <summary>
    /// Updates the short-handed save percentage
    /// </summary>
    private void UpdateShortHandedSavePercentage()
    {
        ShortHandedSavePercentage = ShortHandedShotsAgainst > 0 ? (decimal)ShortHandedSaves / ShortHandedShotsAgainst * 100 : 0;
    }
}
