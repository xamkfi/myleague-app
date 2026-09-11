using Domain.Entities;
using System;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents comprehensive player statistics for a specific season
/// </summary>
public class FloorballPlayerSeasonStatistics : BaseEntity
{
    /// <summary>
    /// Gets the ID of the player these statistics belong to
    /// </summary>
    public Guid PlayerId { get; private set; }

    /// <summary>
    /// Gets the player these statistics belong to
    /// </summary>
    public FloorballPlayer Player { get; private set; }

    /// <summary>
    /// Gets the ID of the team the player played for during this season
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the team the player played for during this season
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

    // Basic statistics
    /// <summary>
    /// Gets the number of games played
    /// </summary>
    public int GamesPlayed { get; private set; }

    /// <summary>
    /// Gets the number of goals scored
    /// </summary>
    public int Goals { get; private set; }

    /// <summary>
    /// Gets the number of assists made
    /// </summary>
    public int Assists { get; private set; }

    /// <summary>
    /// Gets the total points (goals + assists)
    /// </summary>
    public int Points { get; private set; }

    /// <summary>
    /// Gets the penalty minutes
    /// </summary>
    public int PenaltyMinutes { get; private set; }

    /// <summary>
    /// Gets the plus/minus rating
    /// </summary>
    public int PlusMinusRating { get; private set; }

    // Shot statistics
    /// <summary>
    /// Gets the number of shots on goal
    /// </summary>
    public int ShotsOnGoal { get; private set; }

    /// <summary>
    /// Gets the shooting percentage
    /// </summary>
    public decimal ShotPercentage { get; private set; }

    // Power play statistics
    /// <summary>
    /// Gets the number of power play goals
    /// </summary>
    public int PowerPlayGoals { get; private set; }

    /// <summary>
    /// Gets the number of power play assists
    /// </summary>
    public int PowerPlayAssists { get; private set; }

    // Short-handed statistics
    /// <summary>
    /// Gets the number of short-handed goals
    /// </summary>
    public int ShortHandedGoals { get; private set; }

    /// <summary>
    /// Gets the number of short-handed assists
    /// </summary>
    public int ShortHandedAssists { get; private set; }

    // Special goals
    /// <summary>
    /// Gets the number of game-winning goals
    /// </summary>
    public int GameWinningGoals { get; private set; }

    /// <summary>
    /// Gets the number of overtime goals
    /// </summary>
    public int OvertimeGoals { get; private set; }

    // Faceoff statistics (for centers)
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

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballPlayerSeasonStatistics()
    {
        Player = null!;
        Team = null!;
        Competition = null!;
    }

    /// <summary>
    /// Initializes a new instance of FloorballPlayerSeasonStatistics
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="teamId">The team ID</param>
    /// <param name="competitionId">The competition ID</param>
    public FloorballPlayerSeasonStatistics(Guid playerId, Guid teamId, Guid competitionId)
    {
        PlayerId = playerId;
        TeamId = teamId;
        CompetitionId = competitionId;
        Player = null!;
        Team = null!;
        Competition = null!;
        
        // Initialize all statistics to zero
        GamesPlayed = 0;
        Goals = 0;
        Assists = 0;
        Points = 0;
        PenaltyMinutes = 0;
        PlusMinusRating = 0;
        ShotsOnGoal = 0;
        ShotPercentage = 0;
        PowerPlayGoals = 0;
        PowerPlayAssists = 0;
        ShortHandedGoals = 0;
        ShortHandedAssists = 0;
        GameWinningGoals = 0;
        OvertimeGoals = 0;
        FaceoffWins = 0;
        FaceoffAttempts = 0;
        FaceoffPercentage = 0;
    }

    /// <summary>
    /// Records a game played by this player
    /// </summary>
    public void RecordGamePlayed()
    {
        GamesPlayed++;
    }

    /// <summary>
    /// Reverts a previous <see cref="RecordGamePlayed"/> call. Used when the match the player
    /// participated in is reopened from Completed back to InProgress, so the GamesPlayed counter
    /// is not double-incremented when the match is later finished again.
    /// </summary>
    public void RemoveGamePlayed()
    {
        if (GamesPlayed > 0) GamesPlayed--;
    }

    /// <summary>
    /// Records a goal scored by this player
    /// </summary>
    /// <param name="isPowerPlay">Whether it was a power play goal</param>
    /// <param name="isShortHanded">Whether it was a short-handed goal</param>
    /// <param name="isGameWinning">Whether it was a game-winning goal</param>
    /// <param name="isOvertime">Whether it was an overtime goal</param>
    public void RecordGoal(bool isPowerPlay = false, bool isShortHanded = false, bool isGameWinning = false, bool isOvertime = false)
    {
        Goals++;
        Points = Goals + Assists;
        
        if (isPowerPlay) PowerPlayGoals++;
        if (isShortHanded) ShortHandedGoals++;
        if (isGameWinning) GameWinningGoals++;
        if (isOvertime) OvertimeGoals++;
        
        UpdateShotPercentage();
    }

    /// <summary>
    /// Records an assist by this player
    /// </summary>
    /// <param name="isPowerPlay">Whether it was a power play assist</param>
    /// <param name="isShortHanded">Whether it was a short-handed assist</param>
    public void RecordAssist(bool isPowerPlay = false, bool isShortHanded = false)
    {
        Assists++;
        Points = Goals + Assists;
        
        if (isPowerPlay) PowerPlayAssists++;
        if (isShortHanded) ShortHandedAssists++;
    }

    /// <summary>
    /// Records penalty minutes for this player
    /// </summary>
    /// <param name="minutes">The penalty minutes to add</param>
    public void RecordPenaltyMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentException("Penalty minutes cannot be negative.", nameof(minutes));

        PenaltyMinutes += minutes;
    }

    /// <summary>
    /// Removes a goal from this player's season statistics
    /// </summary>
    /// <param name="isPowerPlay">Whether it was a power play goal</param>
    /// <param name="isShortHanded">Whether it was a short-handed goal</param>
    /// <param name="isGameWinning">Whether it was a game-winning goal</param>
    /// <param name="isOvertime">Whether it was an overtime goal</param>
    public void RemoveGoal(bool isPowerPlay = false, bool isShortHanded = false, bool isGameWinning = false, bool isOvertime = false)
    {
        if (Goals > 0)
        {
            Goals--;
            Points = Goals + Assists;
        }

        if (isPowerPlay && PowerPlayGoals > 0) PowerPlayGoals--;
        if (isShortHanded && ShortHandedGoals > 0) ShortHandedGoals--;
        if (isGameWinning && GameWinningGoals > 0) GameWinningGoals--;
        if (isOvertime && OvertimeGoals > 0) OvertimeGoals--;

        UpdateShotPercentage();
    }

    /// <summary>
    /// Removes an assist from this player's season statistics
    /// </summary>
    /// <param name="isPowerPlay">Whether it was a power play assist</param>
    /// <param name="isShortHanded">Whether it was a short-handed assist</param>
    public void RemoveAssist(bool isPowerPlay = false, bool isShortHanded = false)
    {
        if (Assists > 0)
        {
            Assists--;
            Points = Goals + Assists;
        }

        if (isPowerPlay && PowerPlayAssists > 0) PowerPlayAssists--;
        if (isShortHanded && ShortHandedAssists > 0) ShortHandedAssists--;
    }

    /// <summary>
    /// Removes penalty minutes from this player
    /// </summary>
    /// <param name="minutes">The penalty minutes to remove</param>
    public void RemovePenaltyMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentException("Penalty minutes to remove cannot be negative.", nameof(minutes));

        PenaltyMinutes = Math.Max(0, PenaltyMinutes - minutes);
    }

    /// <summary>
    /// Records shots on goal by this player
    /// </summary>
    /// <param name="shots">The number of shots to add</param>
    public void RecordShotsOnGoal(int shots)
    {
        if (shots < 0)
            throw new ArgumentException("Shots cannot be negative.", nameof(shots));
        
        ShotsOnGoal += shots;
        UpdateShotPercentage();
    }

    /// <summary>
    /// Updates the plus/minus rating
    /// </summary>
    /// <param name="change">The change in rating (+1 for goals for, -1 for goals against)</param>
    public void UpdatePlusMinusRating(int change)
    {
        PlusMinusRating += change;
    }

    /// <summary>
    /// Records faceoff results
    /// </summary>
    /// <param name="wins">Number of faceoffs won</param>
    /// <param name="attempts">Total number of faceoffs taken</param>
    public void RecordFaceoffs(int wins, int attempts)
    {
        if (wins < 0 || attempts < 0 || wins > attempts)
            throw new ArgumentException("Invalid faceoff statistics.");
        
        FaceoffWins += wins;
        FaceoffAttempts += attempts;
        FaceoffPercentage = FaceoffAttempts > 0 ? (decimal)FaceoffWins / FaceoffAttempts * 100 : 0;
    }

    /// <summary>
    /// Updates the shooting percentage based on current goals and shots
    /// </summary>
    private void UpdateShotPercentage()
    {
        ShotPercentage = ShotsOnGoal > 0 ? (decimal)Goals / ShotsOnGoal * 100 : 0;
    }
}
