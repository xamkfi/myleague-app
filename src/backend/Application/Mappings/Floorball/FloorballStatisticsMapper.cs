using Application.DTOs.Floorball;
using Domain.Entities.Floorball;

namespace Application.Mappings.Floorball;

/// <summary>
/// Mapper for converting floorball statistics entities to DTOs
/// </summary>
public static class FloorballStatisticsMapper
{
    /// <summary>
    /// Converts FloorballTeamSeasonStatistics entity to DTO
    /// </summary>
    /// <param name="entity">The statistics entity</param>
    /// <param name="teamName">Optional team name</param>
    /// <param name="seasonName">Optional season name</param>
    /// <returns>Statistics DTO</returns>
    public static FloorballTeamSeasonStatisticsDto ToDto(FloorballTeamSeasonStatistics entity, string? teamName = null, string? seasonName = null)
    {
        return new FloorballTeamSeasonStatisticsDto
        {
            Id = entity.Id,
            TeamId = entity.TeamId,
            SeasonId = entity.SeasonId,
            TeamName = entity.Team.Name ?? string.Empty,
            TeamLogo = entity.Team.LogoUrl,
            SeasonName = entity.Season.Name ?? string.Empty,
            GamesPlayed = entity.GamesPlayed,
            Wins = entity.Wins,
            Losses = entity.Losses,
            Ties = entity.Ties,
            Points = entity.Points,
            GoalsFor = entity.GoalsFor,
            GoalsAgainst = entity.GoalsAgainst,
            GoalDifference = entity.GoalDifference,
            ShotsFor = entity.ShotsFor,
            ShotsAgainst = entity.ShotsAgainst,
            ShotPercentage = entity.ShotPercentage,
            PowerPlayGoals = entity.PowerPlayGoals,
            PowerPlayOpportunities = entity.PowerPlayOpportunities,
            PowerPlayPercentage = entity.PowerPlayPercentage,
            ShortHandedGoals = entity.ShortHandedGoals,
            PenaltyKillOpportunities = entity.PenaltyKillOpportunities,
            PenaltyKillPercentage = entity.PenaltyKillPercentage,
            PenaltyMinutes = entity.PenaltyMinutes,
            FaceoffWins = entity.FaceoffWins,
            FaceoffAttempts = entity.FaceoffAttempts,
            FaceoffPercentage = entity.FaceoffPercentage,
            HomeWins = entity.HomeWins,
            HomeLosses = entity.HomeLosses,
            AwayWins = entity.AwayWins,
            AwayLosses = entity.AwayLosses
        };
    }

    /// <summary>
    /// Converts FloorballPlayerSeasonStatistics entity to DTO
    /// </summary>
    /// <param name="entity">The statistics entity</param>
    /// <param name="playerName">Optional player name</param>
    /// <param name="teamName">Optional team name</param>
    /// <param name="seasonName">Optional season name</param>
    /// <returns>Statistics DTO</returns>
    public static FloorballPlayerSeasonStatisticsDto ToDto(FloorballPlayerSeasonStatistics entity, string? playerName = null, string? teamName = null, string? seasonName = null)
    {
        return new FloorballPlayerSeasonStatisticsDto
        {
            Id = entity.Id,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            SeasonId = entity.SeasonId,
            PlayerName = playerName ?? string.Empty,
            TeamName = teamName ?? string.Empty,
            SeasonName = seasonName ?? string.Empty,
            GamesPlayed = entity.GamesPlayed,
            Goals = entity.Goals,
            Assists = entity.Assists,
            Points = entity.Points,
            PenaltyMinutes = entity.PenaltyMinutes,
            PlusMinusRating = entity.PlusMinusRating,
            ShotsOnGoal = entity.ShotsOnGoal,
            ShotPercentage = entity.ShotPercentage,
            PowerPlayGoals = entity.PowerPlayGoals,
            PowerPlayAssists = entity.PowerPlayAssists,
            ShortHandedGoals = entity.ShortHandedGoals,
            ShortHandedAssists = entity.ShortHandedAssists,
            GameWinningGoals = entity.GameWinningGoals,
            OvertimeGoals = entity.OvertimeGoals,
            FaceoffWins = entity.FaceoffWins,
            FaceoffAttempts = entity.FaceoffAttempts,
            FaceoffPercentage = entity.FaceoffPercentage
        };
    }

    /// <summary>
    /// Converts FloorballMatchTeamStatistics entity to DTO
    /// </summary>
    /// <param name="entity">The statistics entity</param>
    /// <param name="teamName">Optional team name</param>
    /// <returns>Statistics DTO</returns>
    public static FloorballMatchTeamStatisticsDto ToDto(FloorballMatchTeamStatistics entity, string? teamName = null)
    {
        return new FloorballMatchTeamStatisticsDto
        {
            Id = entity.Id,
            MatchId = entity.MatchId,
            TeamId = entity.TeamId,
            TeamName = teamName ?? string.Empty,
            ShotsOnGoal = entity.ShotsOnGoal,
            ShotsTotal = entity.ShotsTotal,
            ShotPercentage = entity.ShotPercentage,
            FaceoffWins = entity.FaceoffWins,
            FaceoffAttempts = entity.FaceoffAttempts,
            FaceoffPercentage = entity.FaceoffPercentage,
            PowerPlayOpportunities = entity.PowerPlayOpportunities,
            PowerPlayGoals = entity.PowerPlayGoals,
            PowerPlayMinutes = entity.PowerPlayMinutes,
            PenaltyKillOpportunities = entity.PenaltyKillOpportunities,
            PenaltyKillSuccess = entity.PenaltyKillSuccess,
            ShortHandedGoals = entity.ShortHandedGoals,
            PenaltyMinutes = entity.PenaltyMinutes,
            Hits = entity.Hits,
            BlockedShots = entity.BlockedShots,
            Takeaways = entity.Takeaways,
            Giveaways = entity.Giveaways
        };
    }

    /// <summary>
    /// Converts FloorballGoalieSeasonStatistics entity to DTO
    /// </summary>
    /// <param name="entity">The statistics entity</param>
    /// <param name="playerName">Optional player name</param>
    /// <param name="teamName">Optional team name</param>
    /// <param name="seasonName">Optional season name</param>
    /// <returns>Statistics DTO</returns>
    public static FloorballGoalieSeasonStatisticsDto ToDto(FloorballGoalieSeasonStatistics entity, string? playerName = null, string? teamName = null, string? seasonName = null)
    {
        return new FloorballGoalieSeasonStatisticsDto
        {
            Id = entity.Id,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            SeasonId = entity.SeasonId,
            PlayerName = playerName ?? string.Empty,
            TeamName = teamName ?? string.Empty,
            SeasonName = seasonName ?? string.Empty,
            GamesPlayed = entity.GamesPlayed,
            GamesStarted = entity.GamesStarted,
            Wins = entity.Wins,
            Losses = entity.Losses,
            Ties = entity.Ties,
            Saves = entity.Saves,
            ShotsAgainst = entity.ShotsAgainst,
            SavePercentage = entity.SavePercentage,
            GoalsAgainst = entity.GoalsAgainst,
            GoalsAgainstAverage = entity.GoalsAgainstAverage,
            Shutouts = entity.Shutouts,
            MinutesPlayed = entity.MinutesPlayed,
            PowerPlaySaves = entity.PowerPlaySaves,
            PowerPlayShotsAgainst = entity.PowerPlayShotsAgainst,
            PowerPlaySavePercentage = entity.PowerPlaySavePercentage,
            ShortHandedSaves = entity.ShortHandedSaves,
            ShortHandedShotsAgainst = entity.ShortHandedShotsAgainst,
            ShortHandedSavePercentage = entity.ShortHandedSavePercentage
        };
    }
}
