namespace Application.Features.Hockey.Matches.DTOs;

/// <summary>
/// Calendar and schedule list row. Omits events, lines, and on-ice state.
/// </summary>
public record HockeyMatchListDto(
    Guid Id,
    DateTime ScheduledStartTime,
    string Status,
    string MatchType,
    string? Venue,
    Guid? CompetitionId,
    string? CompetitionName,
    Guid? HomeTeamId,
    Guid? AwayTeamId,
    string? HomeTeamName,
    string? AwayTeamName,
    int HomeScore,
    int AwayScore);
