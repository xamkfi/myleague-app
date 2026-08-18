using Domain.Enums.Hockey.Statistics;

namespace Application.Features.Hockey.Statistics.DTOs;

/// <summary>
/// Full match box score: teams, skaters, and goalies.
/// </summary>
public class HockeyMatchStatisticsDto
{
    public Guid MatchId { get; set; }
    public List<HockeyMatchTeamStatisticsDto> Teams { get; set; } = new();
    public List<HockeyMatchPlayerStatisticsDto> Players { get; set; } = new();
    public List<HockeyGoalieMatchStatisticsDto> Goalies { get; set; } = new();
}

/// <summary>
/// Per-match team statistics.
/// </summary>
public class HockeyMatchTeamStatisticsDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid MatchTeamId { get; set; }
    public Guid TeamId { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int ShotsOnGoal { get; set; }
    public int ShotAttempts { get; set; }
    public int MissedShots { get; set; }
    public int BlockedShotAttempts { get; set; }
    public decimal ShotPercentage { get; set; }
    public int Saves { get; set; }
    public int ShotsAgainst { get; set; }
    public decimal TeamSavePercentage { get; set; }
    public int FaceoffWins { get; set; }
    public int FaceoffAttempts { get; set; }
    public decimal FaceoffPercentage { get; set; }
    public int PowerPlayOpportunities { get; set; }
    public int PowerPlayGoals { get; set; }
    public decimal PowerPlayPercentage { get; set; }
    public int PenaltyKillOpportunities { get; set; }
    public int PenaltyKillSuccesses { get; set; }
    public decimal PenaltyKillPercentage { get; set; }
    public int Penalties { get; set; }
    public int PenaltyMinutes { get; set; }
    public int Hits { get; set; }
    public int BlockedShots { get; set; }
    public int Takeaways { get; set; }
    public int Giveaways { get; set; }
}

/// <summary>
/// Per-match skater statistics.
/// </summary>
public class HockeyMatchPlayerStatisticsDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid MatchTeamId { get; set; }
    public Guid MatchActivePlayerId { get; set; }
    public Guid TeamPlayerId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public int GamesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Points { get; set; }
    public int PenaltyMinutes { get; set; }
    public int PlusMinusRating { get; set; }
    public int ShotsOnGoal { get; set; }
    public int ShotAttempts { get; set; }
    public decimal ShotPercentage { get; set; }
    public int FaceoffWins { get; set; }
    public int FaceoffAttempts { get; set; }
    public decimal FaceoffPercentage { get; set; }
    public int Hits { get; set; }
    public int BlockedShots { get; set; }
    public int Takeaways { get; set; }
    public int Giveaways { get; set; }
    public int TimeOnIceSeconds { get; set; }
    public int Shifts { get; set; }
}

/// <summary>
/// Per-match goalie statistics.
/// </summary>
public class HockeyGoalieMatchStatisticsDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid MatchTeamId { get; set; }
    public Guid MatchActivePlayerId { get; set; }
    public Guid TeamPlayerId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public bool WasStarter { get; set; }
    public string Decision { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
    public int GamesStarted { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int OvertimeLosses { get; set; }
    public int ShootoutLosses { get; set; }
    public int NoDecisions { get; set; }
    public int Saves { get; set; }
    public int ShotsAgainst { get; set; }
    public decimal SavePercentage { get; set; }
    public int GoalsAgainst { get; set; }
    public decimal GoalsAgainstAverage { get; set; }
    public int Shutouts { get; set; }
    public int MinutesPlayed { get; set; }
    public List<HockeyGoaliePeriodStatisticsDto> Periods { get; set; } = new();
}

/// <summary>
/// Per-period goalie statistics within a match.
/// </summary>
public class HockeyGoaliePeriodStatisticsDto
{
    public Guid Id { get; set; }
    public int PeriodNumber { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    public int TimeOnIceSeconds { get; set; }
    public int ShotsAgainst { get; set; }
    public int Saves { get; set; }
    public int GoalsAgainst { get; set; }
    public decimal SavePercentage { get; set; }
}

/// <summary>
/// Aggregated team standings row.
/// </summary>
public class HockeyTeamCompetitionStatisticsDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid CompetitionId { get; set; }
    public HockeyStatisticsScope Scope { get; set; }
    public Guid? CompetitionDivisionId { get; set; }
    public Guid? TournamentGroupId { get; set; }
    public Guid? PlayoffSeriesId { get; set; }
    public int GamesPlayed { get; set; }
    public int RegulationWins { get; set; }
    public int OvertimeWins { get; set; }
    public int ShootoutWins { get; set; }
    public int RegulationLosses { get; set; }
    public int OvertimeLosses { get; set; }
    public int ShootoutLosses { get; set; }
    public int Ties { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Points { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalDifference { get; set; }
    public int ShotsFor { get; set; }
    public int ShotsAgainst { get; set; }
    public decimal ShotPercentage { get; set; }
    public int PowerPlayGoals { get; set; }
    public int PowerPlayOpportunities { get; set; }
    public decimal PowerPlayPercentage { get; set; }
    public int PenaltyKillOpportunities { get; set; }
    public int PenaltyKillSuccesses { get; set; }
    public decimal PenaltyKillPercentage { get; set; }
    public int PenaltyMinutes { get; set; }
    public int FaceoffWins { get; set; }
    public int FaceoffAttempts { get; set; }
    public decimal FaceoffPercentage { get; set; }
    public int HomeWins { get; set; }
    public int HomeLosses { get; set; }
    public int AwayWins { get; set; }
    public int AwayLosses { get; set; }
    public int StandingRank { get; set; }
}

/// <summary>
/// Aggregated skater competition statistics.
/// </summary>
public class HockeyPlayerCompetitionStatisticsDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public Guid TeamPlayerId { get; set; }
    public Guid CompetitionId { get; set; }
    public HockeyStatisticsScope Scope { get; set; }
    public Guid? CompetitionDivisionId { get; set; }
    public Guid? TournamentGroupId { get; set; }
    public Guid? PlayoffSeriesId { get; set; }
    public int GamesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Points { get; set; }
    public int PenaltyMinutes { get; set; }
    public int PlusMinusRating { get; set; }
    public int ShotsOnGoal { get; set; }
    public int ShotAttempts { get; set; }
    public decimal ShotPercentage { get; set; }
    public int FaceoffWins { get; set; }
    public int FaceoffAttempts { get; set; }
    public decimal FaceoffPercentage { get; set; }
    public int Hits { get; set; }
    public int BlockedShots { get; set; }
    public int Takeaways { get; set; }
    public int Giveaways { get; set; }
    public int TimeOnIceSeconds { get; set; }
    public int Shifts { get; set; }
}

/// <summary>
/// Aggregated goalie competition statistics.
/// </summary>
public class HockeyGoalieCompetitionStatisticsDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public Guid TeamPlayerId { get; set; }
    public Guid CompetitionId { get; set; }
    public HockeyStatisticsScope Scope { get; set; }
    public Guid? CompetitionDivisionId { get; set; }
    public Guid? TournamentGroupId { get; set; }
    public Guid? PlayoffSeriesId { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesStarted { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int OvertimeLosses { get; set; }
    public int ShootoutLosses { get; set; }
    public int NoDecisions { get; set; }
    public int Saves { get; set; }
    public int ShotsAgainst { get; set; }
    public decimal SavePercentage { get; set; }
    public int GoalsAgainst { get; set; }
    public decimal GoalsAgainstAverage { get; set; }
    public int Shutouts { get; set; }
    public int MinutesPlayed { get; set; }
}

/// <summary>
/// Top scorer leaderboard row.
/// </summary>
public class HockeyTopScorerDto
{
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public int GamesPlayed { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int Points { get; set; }
}

/// <summary>
/// Top goalie leaderboard row.
/// </summary>
public class HockeyTopGoalieDto
{
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public decimal SavePercentage { get; set; }
    public decimal GoalsAgainstAverage { get; set; }
    public int Shutouts { get; set; }
}

/// <summary>
/// Competition statistics dashboard summary.
/// </summary>
public class HockeyCompetitionStatisticsSummaryDto
{
    public Guid CompetitionId { get; set; }
    public HockeyStatisticsScope Scope { get; set; }
    public Guid? CompetitionDivisionId { get; set; }
    public Guid? TournamentGroupId { get; set; }
    public Guid? PlayoffSeriesId { get; set; }
    public int TeamCount { get; set; }
    public int PlayerCount { get; set; }
    public int GoalieCount { get; set; }
    public List<HockeyTeamCompetitionStatisticsDto> Standings { get; set; } = new();
    public List<HockeyTopScorerDto> TopScorers { get; set; } = new();
    public List<HockeyTopGoalieDto> TopGoalies { get; set; } = new();
}

/// <summary>
/// Playoff series statistics snapshot.
/// </summary>
public class HockeyPlayoffSeriesStatisticsDto
{
    public Guid CompetitionId { get; set; }
    public Guid PlayoffSeriesId { get; set; }
    public List<HockeyTeamCompetitionStatisticsDto> Teams { get; set; } = new();
    public List<HockeyPlayerCompetitionStatisticsDto> Players { get; set; } = new();
    public List<HockeyGoalieCompetitionStatisticsDto> Goalies { get; set; } = new();
}
