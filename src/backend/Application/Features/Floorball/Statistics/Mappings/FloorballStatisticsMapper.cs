using System.Collections.Immutable;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using Domain.Entities.Common;
using Domain.Entities.Floorball;

namespace Application.Features.Floorball.Statistics.Mappings;

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
    public static FloorballTeamSeasonStatisticsDto ToDto(FloorballTeamSeasonStatistics entity)
    {
        return new FloorballTeamSeasonStatisticsDto
        {
            Id = entity.Id,
            TeamId = entity.TeamId,
            CompetitionId = entity.CompetitionId,
            TeamName = entity.Team.Name ?? string.Empty,
            TeamLogo = entity.Team.LogoUrl,
            SeasonName = entity.Competition.Name ?? string.Empty,
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
    public static FloorballPlayerSeasonStatisticsDto ToDto(FloorballPlayerSeasonStatistics entity, string? playerName = null)
    {
        return new FloorballPlayerSeasonStatisticsDto
        {
            Id = entity.Id,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            CompetitionId = entity.CompetitionId,
            PlayerName = playerName ?? string.Empty,
            TeamName = entity.Team.Name,
            SeasonName = entity.Competition.Name,
            TeamLogo = entity.Team.LogoUrl?.ToString(),
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
    public static FloorballGoalieSeasonStatisticsDto ToDto(FloorballGoalieSeasonStatistics entity, string? playerName = null)
    {
        return new FloorballGoalieSeasonStatisticsDto
        {
            Id = entity.Id,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            CompetitionId = entity.CompetitionId,
            PlayerName = playerName ?? string.Empty,
            TeamName = entity.Team.Name ?? string.Empty,
            SeasonName = entity.Competition.Name ?? string.Empty,
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

    public static FloorballPlayerProfileDto ToDto(FloorballPlayer player, Person person, List<FloorballPlayerSeasonStatistics> statistics, List<FloorballGoalieSeasonStatistics> goalieStatistics)
    {
        return new FloorballPlayerProfileDto
        {
            Player = new FloorballPlayerPublicDto(
                player.Id,
                player.PersonId,
                PersonMapper.ToPublicDto(person),
                player.IsActive,
                player.Position.PrimaryPosition,
                player.CareerGoals,
                player.CareerAssists,
                null
            ),
            SeasonStatistics = statistics.Select(stat => new FloorballPlayerSeasonStatisticsDto
            {
                Id = stat.Id,
                PlayerId = stat.PlayerId,
                TeamId = stat.TeamId,
                CompetitionId = stat.CompetitionId,
                PlayerName = "",
                TeamName = stat.Team?.Name ?? string.Empty,
                TeamLogo = stat.Team?.LogoUrl?.ToString(),
                SeasonName = stat.Competition?.Name ?? string.Empty,
                GamesPlayed = stat.GamesPlayed,
                Goals = stat.Goals,
                Assists = stat.Assists,
                Points = stat.Points,
                PenaltyMinutes = stat.PenaltyMinutes,
                PlusMinusRating = stat.PlusMinusRating,
                ShotsOnGoal = stat.ShotsOnGoal,
                ShotPercentage = stat.ShotPercentage,
                PowerPlayGoals = stat.PowerPlayGoals,
                PowerPlayAssists = stat.PowerPlayAssists,
                ShortHandedGoals = stat.ShortHandedGoals,
                ShortHandedAssists = stat.ShortHandedAssists,
                GameWinningGoals = stat.GameWinningGoals,
                OvertimeGoals = stat.OvertimeGoals,
                FaceoffWins = stat.FaceoffWins,
                FaceoffAttempts = stat.FaceoffAttempts,
                FaceoffPercentage = stat.FaceoffPercentage
            }).ToList(),
            SeasonStatisticsForGoalie = goalieStatistics.Select(stat => new FloorballGoalieSeasonStatisticsDto
            {
                Id = stat.Id,
                PlayerId = stat.PlayerId,
                TeamId = stat.TeamId,
                CompetitionId = stat.CompetitionId,
                PlayerName = person.FullName,
                TeamName = stat.Team?.Name ?? string.Empty,
                SeasonName = stat.Competition?.Name ?? string.Empty,
                GamesPlayed = stat.GamesPlayed,
                GamesStarted = stat.GamesStarted,
                Wins = stat.Wins,
                Losses = stat.Losses,
                Ties = stat.Ties,
                Saves = stat.Saves,
                ShotsAgainst = stat.ShotsAgainst,
                SavePercentage = stat.SavePercentage,
                GoalsAgainst = stat.GoalsAgainst,
                GoalsAgainstAverage = stat.GoalsAgainstAverage,
                Shutouts = stat.Shutouts,
                MinutesPlayed = stat.MinutesPlayed,
                PowerPlaySaves = stat.PowerPlaySaves,
                PowerPlayShotsAgainst = stat.PowerPlayShotsAgainst,
                PowerPlaySavePercentage = stat.PowerPlaySavePercentage,
                ShortHandedSaves = stat.ShortHandedSaves,
                ShortHandedShotsAgainst = stat.ShortHandedShotsAgainst,
                ShortHandedSavePercentage = stat.ShortHandedSavePercentage
            }).ToList()
        };
    }
}
