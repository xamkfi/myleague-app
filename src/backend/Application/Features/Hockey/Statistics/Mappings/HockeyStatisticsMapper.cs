using Application.Features.Hockey.Statistics.DTOs;
using Domain.Entities.Hockey.Statistics;

namespace Application.Features.Hockey.Statistics.Mappings;

/// <summary>
/// Maps hockey statistics entities to DTOs.
/// </summary>
public static class HockeyStatisticsMapper
{
    public static HockeyMatchStatisticsDto ToMatchStatisticsDto(
        Guid matchId,
        IEnumerable<HockeyMatchTeamStatistics> teams,
        IEnumerable<HockeyMatchPlayerStatistics> players,
        IEnumerable<HockeyGoalieMatchStatistics> goalies) =>
        new()
        {
            MatchId = matchId,
            Teams = teams.Select(ToDto).ToList(),
            Players = players.Select(ToDto).ToList(),
            Goalies = goalies.Select(ToDto).ToList()
        };

    public static HockeyMatchTeamStatisticsDto ToDto(HockeyMatchTeamStatistics entity) =>
        new()
        {
            Id = entity.Id,
            MatchId = entity.MatchId,
            MatchTeamId = entity.MatchTeamId,
            TeamId = entity.TeamId,
            GoalsFor = entity.GoalsFor,
            GoalsAgainst = entity.GoalsAgainst,
            ShotsOnGoal = entity.ShotsOnGoal,
            ShotAttempts = entity.ShotAttempts,
            MissedShots = entity.MissedShots,
            BlockedShotAttempts = entity.BlockedShotAttempts,
            ShotPercentage = entity.ShotPercentage,
            Saves = entity.Saves,
            ShotsAgainst = entity.ShotsAgainst,
            TeamSavePercentage = entity.TeamSavePercentage,
            FaceoffWins = entity.FaceoffWins,
            FaceoffAttempts = entity.FaceoffAttempts,
            FaceoffPercentage = entity.FaceoffPercentage,
            PowerPlayOpportunities = entity.PowerPlayOpportunities,
            PowerPlayGoals = entity.PowerPlayGoals,
            PowerPlayPercentage = entity.PowerPlayPercentage,
            PenaltyKillOpportunities = entity.PenaltyKillOpportunities,
            PenaltyKillSuccesses = entity.PenaltyKillSuccesses,
            PenaltyKillPercentage = entity.PenaltyKillPercentage,
            Penalties = entity.Penalties,
            PenaltyMinutes = entity.PenaltyMinutes,
            Hits = entity.Hits,
            BlockedShots = entity.BlockedShots,
            Takeaways = entity.Takeaways,
            Giveaways = entity.Giveaways
        };

    public static HockeyMatchPlayerStatisticsDto ToDto(HockeyMatchPlayerStatistics entity) =>
        new()
        {
            Id = entity.Id,
            MatchId = entity.MatchId,
            MatchTeamId = entity.MatchTeamId,
            MatchActivePlayerId = entity.MatchActivePlayerId,
            TeamPlayerId = entity.TeamPlayerId,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            GamesPlayed = entity.GamesPlayed,
            Goals = entity.Goals,
            Assists = entity.Assists,
            Points = entity.Points,
            PenaltyMinutes = entity.PenaltyMinutes,
            PlusMinusRating = entity.PlusMinusRating,
            ShotsOnGoal = entity.ShotsOnGoal,
            ShotAttempts = entity.ShotAttempts,
            ShotPercentage = entity.ShotPercentage,
            FaceoffWins = entity.FaceoffWins,
            FaceoffAttempts = entity.FaceoffAttempts,
            FaceoffPercentage = entity.FaceoffPercentage,
            Hits = entity.Hits,
            BlockedShots = entity.BlockedShots,
            Takeaways = entity.Takeaways,
            Giveaways = entity.Giveaways,
            TimeOnIceSeconds = entity.TimeOnIceSeconds,
            Shifts = entity.Shifts
        };

    public static HockeyGoalieMatchStatisticsDto ToDto(HockeyGoalieMatchStatistics entity) =>
        new()
        {
            Id = entity.Id,
            MatchId = entity.MatchId,
            MatchTeamId = entity.MatchTeamId,
            MatchActivePlayerId = entity.MatchActivePlayerId,
            TeamPlayerId = entity.TeamPlayerId,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            WasStarter = entity.WasStarter,
            Decision = entity.Decision.ToString(),
            GamesPlayed = entity.GamesPlayed,
            GamesStarted = entity.GamesStarted,
            Wins = entity.Wins,
            Losses = entity.Losses,
            OvertimeLosses = entity.OvertimeLosses,
            ShootoutLosses = entity.ShootoutLosses,
            NoDecisions = entity.NoDecisions,
            Saves = entity.Saves,
            ShotsAgainst = entity.ShotsAgainst,
            SavePercentage = entity.SavePercentage,
            GoalsAgainst = entity.GoalsAgainst,
            GoalsAgainstAverage = entity.GoalsAgainstAverage,
            Shutouts = entity.Shutouts,
            MinutesPlayed = entity.MinutesPlayed,
            Periods = entity.PeriodStatistics.Select(ToDto).ToList()
        };

    public static HockeyGoaliePeriodStatisticsDto ToDto(HockeyGoaliePeriodStatistics entity) =>
        new()
        {
            Id = entity.Id,
            PeriodNumber = entity.PeriodNumber,
            PeriodType = entity.PeriodType.ToString(),
            TimeOnIceSeconds = entity.TimeOnIceSeconds,
            ShotsAgainst = entity.ShotsAgainst,
            Saves = entity.Saves,
            GoalsAgainst = entity.GoalsAgainst,
            SavePercentage = entity.SavePercentage
        };

    public static HockeyTeamCompetitionStatisticsDto ToDto(HockeyTeamCompetitionStatistics entity) =>
        new()
        {
            Id = entity.Id,
            TeamId = entity.TeamId,
            CompetitionId = entity.CompetitionId,
            Scope = entity.Scope,
            CompetitionDivisionId = entity.CompetitionDivisionId,
            TournamentGroupId = entity.TournamentGroupId,
            PlayoffSeriesId = entity.PlayoffSeriesId,
            GamesPlayed = entity.GamesPlayed,
            RegulationWins = entity.RegulationWins,
            OvertimeWins = entity.OvertimeWins,
            ShootoutWins = entity.ShootoutWins,
            RegulationLosses = entity.RegulationLosses,
            OvertimeLosses = entity.OvertimeLosses,
            ShootoutLosses = entity.ShootoutLosses,
            Ties = entity.Ties,
            Wins = entity.Wins,
            Losses = entity.Losses,
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
            PenaltyKillOpportunities = entity.PenaltyKillOpportunities,
            PenaltyKillSuccesses = entity.PenaltyKillSuccesses,
            PenaltyKillPercentage = entity.PenaltyKillPercentage,
            PenaltyMinutes = entity.PenaltyMinutes,
            FaceoffWins = entity.FaceoffWins,
            FaceoffAttempts = entity.FaceoffAttempts,
            FaceoffPercentage = entity.FaceoffPercentage,
            HomeWins = entity.HomeWins,
            HomeLosses = entity.HomeLosses,
            AwayWins = entity.AwayWins,
            AwayLosses = entity.AwayLosses,
            StandingRank = entity.StandingRank
        };

    public static HockeyPlayerCompetitionStatisticsDto ToDto(HockeyPlayerCompetitionStatistics entity) =>
        new()
        {
            Id = entity.Id,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            TeamPlayerId = entity.TeamPlayerId,
            CompetitionId = entity.CompetitionId,
            Scope = entity.Scope,
            CompetitionDivisionId = entity.CompetitionDivisionId,
            TournamentGroupId = entity.TournamentGroupId,
            PlayoffSeriesId = entity.PlayoffSeriesId,
            GamesPlayed = entity.GamesPlayed,
            Goals = entity.Goals,
            Assists = entity.Assists,
            Points = entity.Points,
            PenaltyMinutes = entity.PenaltyMinutes,
            PlusMinusRating = entity.PlusMinusRating,
            ShotsOnGoal = entity.ShotsOnGoal,
            ShotAttempts = entity.ShotAttempts,
            ShotPercentage = entity.ShotPercentage,
            FaceoffWins = entity.FaceoffWins,
            FaceoffAttempts = entity.FaceoffAttempts,
            FaceoffPercentage = entity.FaceoffPercentage,
            Hits = entity.Hits,
            BlockedShots = entity.BlockedShots,
            Takeaways = entity.Takeaways,
            Giveaways = entity.Giveaways,
            TimeOnIceSeconds = entity.TimeOnIceSeconds,
            Shifts = entity.Shifts
        };

    public static HockeyGoalieCompetitionStatisticsDto ToDto(HockeyGoalieCompetitionStatistics entity) =>
        new()
        {
            Id = entity.Id,
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            TeamPlayerId = entity.TeamPlayerId,
            CompetitionId = entity.CompetitionId,
            Scope = entity.Scope,
            CompetitionDivisionId = entity.CompetitionDivisionId,
            TournamentGroupId = entity.TournamentGroupId,
            PlayoffSeriesId = entity.PlayoffSeriesId,
            GamesPlayed = entity.GamesPlayed,
            GamesStarted = entity.GamesStarted,
            Wins = entity.Wins,
            Losses = entity.Losses,
            OvertimeLosses = entity.OvertimeLosses,
            ShootoutLosses = entity.ShootoutLosses,
            NoDecisions = entity.NoDecisions,
            Saves = entity.Saves,
            ShotsAgainst = entity.ShotsAgainst,
            SavePercentage = entity.SavePercentage,
            GoalsAgainst = entity.GoalsAgainst,
            GoalsAgainstAverage = entity.GoalsAgainstAverage,
            Shutouts = entity.Shutouts,
            MinutesPlayed = entity.MinutesPlayed
        };

    public static HockeyTopScorerDto ToTopScorerDto(HockeyPlayerCompetitionStatistics entity) =>
        new()
        {
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            GamesPlayed = entity.GamesPlayed,
            Goals = entity.Goals,
            Assists = entity.Assists,
            Points = entity.Points
        };

    public static HockeyTopGoalieDto ToTopGoalieDto(HockeyGoalieCompetitionStatistics entity) =>
        new()
        {
            PlayerId = entity.PlayerId,
            TeamId = entity.TeamId,
            GamesPlayed = entity.GamesPlayed,
            Wins = entity.Wins,
            SavePercentage = entity.SavePercentage,
            GoalsAgainstAverage = entity.GoalsAgainstAverage,
            Shutouts = entity.Shutouts
        };
}
