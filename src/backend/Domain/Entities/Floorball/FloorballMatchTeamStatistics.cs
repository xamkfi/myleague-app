using Domain.Entities;
using System;

namespace Domain.Entities.Floorball;

/// <summary>
/// Represents detailed team statistics for a specific match
/// </summary>
public class FloorballMatchTeamStatistics : BaseEntity
{
    /// <summary>
    /// Gets the ID of the match these statistics belong to
    /// </summary>
    public Guid MatchId { get; private set; }

    /// <summary>
    /// Gets the match these statistics belong to
    /// </summary>
    public FloorballMatch Match { get; private set; }

    /// <summary>
    /// Gets the ID of the team these statistics are for
    /// </summary>
    public Guid TeamId { get; private set; }

    /// <summary>
    /// Gets the team these statistics are for
    /// </summary>
    public FloorballTeam Team { get; private set; }

    // Shot statistics
    /// <summary>
    /// Gets the number of shots on goal
    /// </summary>
    public int ShotsOnGoal { get; private set; }

    /// <summary>
    /// Gets the total number of shots taken (including missed shots)
    /// </summary>
    public int ShotsTotal { get; private set; }

    /// <summary>
    /// Gets the shot percentage
    /// </summary>
    public decimal ShotPercentage { get; private set; }

    // Faceoff statistics
    /// <summary>
    /// Gets the number of faceoffs won
    /// </summary>
    public int FaceoffWins { get; private set; }

    /// <summary>
    /// Gets the total number of faceoffs
    /// </summary>
    public int FaceoffAttempts { get; private set; }

    /// <summary>
    /// Gets the faceoff win percentage
    /// </summary>
    public decimal FaceoffPercentage { get; private set; }

    // Power play statistics
    /// <summary>
    /// Gets the number of power play opportunities
    /// </summary>
    public int PowerPlayOpportunities { get; private set; }

    /// <summary>
    /// Gets the number of power play goals scored
    /// </summary>
    public int PowerPlayGoals { get; private set; }

    /// <summary>
    /// Gets the total power play minutes
    /// </summary>
    public int PowerPlayMinutes { get; private set; }

    // Penalty kill statistics
    /// <summary>
    /// Gets the number of penalty kill opportunities
    /// </summary>
    public int PenaltyKillOpportunities { get; private set; }

    /// <summary>
    /// Gets the number of successful penalty kills
    /// </summary>
    public int PenaltyKillSuccess { get; private set; }

    /// <summary>
    /// Gets the number of short-handed goals scored
    /// </summary>
    public int ShortHandedGoals { get; private set; }

    // Penalty statistics
    /// <summary>
    /// Gets the total penalty minutes
    /// </summary>
    public int PenaltyMinutes { get; private set; }

    // Physical play statistics
    /// <summary>
    /// Gets the number of hits delivered
    /// </summary>
    public int Hits { get; private set; }

    /// <summary>
    /// Gets the number of shots blocked
    /// </summary>
    public int BlockedShots { get; private set; }

    /// <summary>
    /// Gets the number of takeaways
    /// </summary>
    public int Takeaways { get; private set; }

    /// <summary>
    /// Gets the number of giveaways
    /// </summary>
    public int Giveaways { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private FloorballMatchTeamStatistics()
    {
        Match = null!;
        Team = null!;
    }

    /// <summary>
    /// Initializes a new instance of FloorballMatchTeamStatistics
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <param name="teamId">The team ID</param>
    public FloorballMatchTeamStatistics(Guid matchId, Guid teamId)
    {
        MatchId = matchId;
        TeamId = teamId;
        Match = null!;
        Team = null!;
        
        // Initialize all statistics to zero
        ShotsOnGoal = 0;
        ShotsTotal = 0;
        ShotPercentage = 0;
        FaceoffWins = 0;
        FaceoffAttempts = 0;
        FaceoffPercentage = 0;
        PowerPlayOpportunities = 0;
        PowerPlayGoals = 0;
        PowerPlayMinutes = 0;
        PenaltyKillOpportunities = 0;
        PenaltyKillSuccess = 0;
        ShortHandedGoals = 0;
        PenaltyMinutes = 0;
        Hits = 0;
        BlockedShots = 0;
        Takeaways = 0;
        Giveaways = 0;
    }

    /// <summary>
    /// Updates shot statistics
    /// </summary>
    /// <param name="shotsOnGoal">Shots on goal to add</param>
    /// <param name="shotsTotal">Total shots to add</param>
    public void UpdateShotStatistics(int shotsOnGoal, int shotsTotal)
    {
        if (shotsOnGoal < 0 || shotsTotal < 0 || shotsOnGoal > shotsTotal)
            throw new ArgumentException("Invalid shot statistics.");

        ShotsOnGoal += shotsOnGoal;
        ShotsTotal += shotsTotal;
        UpdateShotPercentage();
    }

    /// <summary>
    /// Updates faceoff statistics
    /// </summary>
    /// <param name="wins">Faceoffs won to add</param>
    /// <param name="attempts">Total faceoffs to add</param>
    public void UpdateFaceoffStatistics(int wins, int attempts)
    {
        if (wins < 0 || attempts < 0 || wins > attempts)
            throw new ArgumentException("Invalid faceoff statistics.");

        FaceoffWins += wins;
        FaceoffAttempts += attempts;
        UpdateFaceoffPercentage();
    }

    /// <summary>
    /// Updates power play statistics
    /// </summary>
    /// <param name="opportunities">Power play opportunities to add</param>
    /// <param name="goals">Power play goals to add</param>
    /// <param name="minutes">Power play minutes to add</param>
    public void UpdatePowerPlayStatistics(int opportunities, int goals, int minutes)
    {
        if (opportunities < 0 || goals < 0 || minutes < 0)
            throw new ArgumentException("Power play statistics cannot be negative.");

        PowerPlayOpportunities += opportunities;
        PowerPlayGoals += goals;
        PowerPlayMinutes += minutes;
    }

    /// <summary>
    /// Updates penalty kill statistics
    /// </summary>
    /// <param name="opportunities">Penalty kill opportunities to add</param>
    /// <param name="successes">Successful penalty kills to add</param>
    /// <param name="shortHandedGoals">Short-handed goals to add</param>
    public void UpdatePenaltyKillStatistics(int opportunities, int successes, int shortHandedGoals)
    {
        if (opportunities < 0 || successes < 0 || shortHandedGoals < 0 || successes > opportunities)
            throw new ArgumentException("Invalid penalty kill statistics.");

        PenaltyKillOpportunities += opportunities;
        PenaltyKillSuccess += successes;
        ShortHandedGoals += shortHandedGoals;
    }

    /// <summary>
    /// Updates penalty minutes
    /// </summary>
    /// <param name="minutes">Penalty minutes to add</param>
    public void UpdatePenaltyMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentException("Penalty minutes cannot be negative.");

        PenaltyMinutes += minutes;
    }

    /// <summary>
    /// Removes shot statistics
    /// </summary>
    /// <param name="shotsOnGoal">Shots on goal to remove</param>
    /// <param name="shotsTotal">Total shots to remove</param>
    public void RemoveShotStatistics(int shotsOnGoal, int shotsTotal)
    {
        if (shotsOnGoal < 0 || shotsTotal < 0 || shotsOnGoal > shotsTotal)
            throw new ArgumentException("Invalid shot statistics to remove.");

        ShotsOnGoal = Math.Max(0, ShotsOnGoal - shotsOnGoal);
        ShotsTotal = Math.Max(0, ShotsTotal - shotsTotal);
        UpdateShotPercentage();
    }

    /// <summary>
    /// Removes penalty minutes
    /// </summary>
    /// <param name="minutes">Penalty minutes to remove</param>
    public void RemovePenaltyMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentException("Penalty minutes to remove cannot be negative.");

        PenaltyMinutes = Math.Max(0, PenaltyMinutes - minutes);
    }

    /// <summary>
    /// Updates physical play statistics
    /// </summary>
    /// <param name="hits">Hits to add</param>
    /// <param name="blockedShots">Blocked shots to add</param>
    /// <param name="takeaways">Takeaways to add</param>
    /// <param name="giveaways">Giveaways to add</param>
    public void UpdatePhysicalStatistics(int hits, int blockedShots, int takeaways, int giveaways)
    {
        if (hits < 0 || blockedShots < 0 || takeaways < 0 || giveaways < 0)
            throw new ArgumentException("Physical statistics cannot be negative.");

        Hits += hits;
        BlockedShots += blockedShots;
        Takeaways += takeaways;
        Giveaways += giveaways;
    }

    /// <summary>
    /// Updates the shot percentage based on current shots and goals
    /// </summary>
    private void UpdateShotPercentage()
    {
        // Note: This assumes goals are tracked elsewhere and passed in when needed
        // For now, we'll calculate based on shots on goal vs total shots
        ShotPercentage = ShotsTotal > 0 ? (decimal)ShotsOnGoal / ShotsTotal * 100 : 0;
    }

    /// <summary>
    /// Updates the faceoff percentage
    /// </summary>
    private void UpdateFaceoffPercentage()
    {
        FaceoffPercentage = FaceoffAttempts > 0 ? (decimal)FaceoffWins / FaceoffAttempts * 100 : 0;
    }

    /// <summary>
    /// Gets the power play percentage
    /// </summary>
    public decimal PowerPlayPercentage => PowerPlayOpportunities > 0 ? (decimal)PowerPlayGoals / PowerPlayOpportunities * 100 : 0;

    /// <summary>
    /// Gets the penalty kill percentage
    /// </summary>
    public decimal PenaltyKillPercentage => PenaltyKillOpportunities > 0 ? (decimal)PenaltyKillSuccess / PenaltyKillOpportunities * 100 : 0;
}
