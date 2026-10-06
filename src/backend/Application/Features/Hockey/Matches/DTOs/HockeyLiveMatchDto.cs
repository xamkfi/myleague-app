namespace Application.Features.Hockey.Matches.DTOs;

/// <summary>
/// Score and clock state for live polling. Omits events, lineups, officials, and period scores.
/// </summary>
public record HockeyLiveMatchDto(
    Guid Id,
    Guid? CompetitionId,
    string MatchType,
    string Status,
    string? ResultType,
    int CurrentPeriodNumber,
    bool WentToOvertime,
    bool WentToShootout,
    DateTime ScheduledStartTime,
    DateTime? ActualStartTime,
    DateTime? ActualEndTime,
    Guid? HomeTeamId,
    Guid? AwayTeamId,
    int HomeScore,
    int AwayScore);
