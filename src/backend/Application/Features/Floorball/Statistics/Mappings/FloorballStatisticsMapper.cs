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
    /// Sentinel competition ID embedded into aggregated DTOs. We pick <see cref="Guid.Empty"/>
    /// instead of one of the source competition IDs because the aggregated row is synthetic and
    /// must not be mistaken for a real <see cref="FloorballCompetition"/> by the frontend (e.g.
    /// when building links to /league/{competitionId}).
    /// </summary>
    public static readonly Guid AggregatedCompetitionId = Guid.Empty;

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
    /// Aggregates <paramref name="rows"/> (a team's per-competition statistics rows) into a single
    /// "career-style" DTO. Sums all counter columns (games, wins, losses, goals, shots, ...) and
    /// recomputes the percentage fields from those summed counters so a shot percentage that
    /// looked correct in any individual row still adds up across the season + tournaments.
    ///
    /// Pass a non-null <paramref name="seasonName"/> to override the synthetic season label that
    /// would otherwise show the source competition's name (we use this to display "Yhteensä" on
    /// the team page).
    /// </summary>
    public static FloorballTeamSeasonStatisticsDto AggregateTeamStatistics(
        Guid teamId,
        IReadOnlyCollection<FloorballTeamSeasonStatistics> rows,
        string teamName,
        Uri? teamLogo,
        string? seasonName = null)
    {
        FloorballTeamSeasonStatisticsDto dto = new FloorballTeamSeasonStatisticsDto
        {
            Id = Guid.Empty,
            TeamId = teamId,
            CompetitionId = AggregatedCompetitionId,
            TeamName = teamName,
            TeamLogo = teamLogo,
            SeasonName = seasonName ?? string.Empty,
            LastFiveForm = Array.Empty<Domain.Enums.Floorball.FloorballGameResult>()
        };

        foreach (FloorballTeamSeasonStatistics row in rows)
        {
            dto.GamesPlayed += row.GamesPlayed;
            dto.Wins += row.Wins;
            dto.Losses += row.Losses;
            dto.Ties += row.Ties;
            dto.Points += row.Points;
            dto.GoalsFor += row.GoalsFor;
            dto.GoalsAgainst += row.GoalsAgainst;
            dto.ShotsFor += row.ShotsFor;
            dto.ShotsAgainst += row.ShotsAgainst;
            dto.PowerPlayGoals += row.PowerPlayGoals;
            dto.PowerPlayOpportunities += row.PowerPlayOpportunities;
            dto.ShortHandedGoals += row.ShortHandedGoals;
            dto.PenaltyKillOpportunities += row.PenaltyKillOpportunities;
            dto.PenaltyMinutes += row.PenaltyMinutes;
            dto.FaceoffWins += row.FaceoffWins;
            dto.FaceoffAttempts += row.FaceoffAttempts;
            dto.HomeWins += row.HomeWins;
            dto.HomeLosses += row.HomeLosses;
            dto.AwayWins += row.AwayWins;
            dto.AwayLosses += row.AwayLosses;
        }

        dto.GoalDifference = dto.GoalsFor - dto.GoalsAgainst;
        dto.ShotPercentage = dto.ShotsFor > 0 ? (decimal)dto.GoalsFor / dto.ShotsFor * 100m : 0m;
        dto.PowerPlayPercentage = dto.PowerPlayOpportunities > 0
            ? (decimal)dto.PowerPlayGoals / dto.PowerPlayOpportunities * 100m
            : 0m;
        // Mirror FloorballTeamSeasonStatistics.UpdateAfterMatch's PK% formula so the aggregated
        // value is consistent with what each row would show on its own.
        dto.PenaltyKillPercentage = dto.PenaltyKillOpportunities > 0
            ? (decimal)(dto.PenaltyKillOpportunities - (dto.GoalsAgainst - dto.PowerPlayGoals)) / dto.PenaltyKillOpportunities * 100m
            : 0m;
        dto.FaceoffPercentage = dto.FaceoffAttempts > 0
            ? (decimal)dto.FaceoffWins / dto.FaceoffAttempts * 100m
            : 0m;

        return dto;
    }

    /// <summary>
    /// Aggregates <paramref name="rows"/> (per-competition stats rows for a single player on a
    /// single team) into a single DTO. Counters are summed; percentage fields are recomputed
    /// from the summed counters. The synthetic row uses a stable surrogate key so the consumer
    /// (React tables) gets a deterministic React key even though no real entity exists.
    /// </summary>
    public static FloorballPlayerSeasonStatisticsDto AggregatePlayerStatistics(
        Guid playerId,
        Guid teamId,
        string playerName,
        string teamName,
        string? teamLogo,
        IReadOnlyCollection<FloorballPlayerSeasonStatistics> rows)
    {
        FloorballPlayerSeasonStatisticsDto dto = new FloorballPlayerSeasonStatisticsDto
        {
            Id = Guid.Empty,
            PlayerId = playerId,
            TeamId = teamId,
            CompetitionId = AggregatedCompetitionId,
            PlayerName = playerName,
            TeamName = teamName,
            TeamLogo = teamLogo,
            SeasonName = string.Empty
        };

        foreach (FloorballPlayerSeasonStatistics row in rows)
        {
            dto.GamesPlayed += row.GamesPlayed;
            dto.Goals += row.Goals;
            dto.Assists += row.Assists;
            dto.Points += row.Points;
            dto.PenaltyMinutes += row.PenaltyMinutes;
            dto.PlusMinusRating += row.PlusMinusRating;
            dto.ShotsOnGoal += row.ShotsOnGoal;
            dto.PowerPlayGoals += row.PowerPlayGoals;
            dto.PowerPlayAssists += row.PowerPlayAssists;
            dto.ShortHandedGoals += row.ShortHandedGoals;
            dto.ShortHandedAssists += row.ShortHandedAssists;
            dto.GameWinningGoals += row.GameWinningGoals;
            dto.OvertimeGoals += row.OvertimeGoals;
            dto.FaceoffWins += row.FaceoffWins;
            dto.FaceoffAttempts += row.FaceoffAttempts;
        }

        dto.ShotPercentage = dto.ShotsOnGoal > 0 ? (decimal)dto.Goals / dto.ShotsOnGoal * 100m : 0m;
        dto.FaceoffPercentage = dto.FaceoffAttempts > 0
            ? (decimal)dto.FaceoffWins / dto.FaceoffAttempts * 100m
            : 0m;

        return dto;
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
