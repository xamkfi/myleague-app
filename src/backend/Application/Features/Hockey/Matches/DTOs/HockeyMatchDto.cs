namespace Application.Features.Hockey.Matches.DTOs;

/// <summary>
/// Summary DTO for a hockey match.
/// </summary>
public record HockeyMatchDto(
    Guid Id,
    Guid? CompetitionId,
    Guid? CompetitionDivisionId,
    Guid? TournamentGroupId,
    Guid? PlayoffSeriesId,
    DateTime ScheduledStartTime,
    DateTime? ActualStartTime,
    DateTime? ActualEndTime,
    string? Venue,
    string MatchType,
    string Status,
    string? ResultType,
    int CurrentPeriodNumber,
    bool WentToOvertime,
    bool WentToShootout,
    Guid? HomeTeamId,
    Guid? AwayTeamId,
    int HomeScore,
    int AwayScore,
    IReadOnlyCollection<HockeyMatchTeamDto> MatchTeams,
    IReadOnlyCollection<HockeyMatchEventDto> Events,
    IReadOnlyCollection<HockeyMatchOfficialDto> Officials,
    IReadOnlyCollection<HockeyPeriodScoreDto> PeriodScores);

/// <summary>
/// One side of a hockey match (home/away).
/// </summary>
public record HockeyMatchTeamDto(
    Guid Id,
    Guid MatchId,
    Guid TeamId,
    Guid? CompetitionTeamId,
    string TeamSlot,
    int Goals,
    bool IsConfirmedRoster,
    IReadOnlyCollection<HockeyMatchActivePlayerDto> ActivePlayers);

/// <summary>
/// Dressed player on a match roster.
/// </summary>
public record HockeyMatchActivePlayerDto(
    Guid Id,
    Guid TeamPlayerId,
    int JerseyNumber,
    string Position,
    bool IsActive,
    bool IsStartingPlayer,
    bool IsGoalie);

/// <summary>
/// Summary of a match event.
/// </summary>
public record HockeyMatchEventDto(
    Guid Id,
    string EventType,
    int PeriodNumber,
    int GameTimeSeconds,
    Guid? MatchTeamId,
    Guid? MatchActivePlayerId,
    string? Description);

/// <summary>
/// Official assigned to a match.
/// </summary>
public record HockeyMatchOfficialDto(
    Guid Id,
    Guid OfficialId,
    string Role,
    bool IsMainOfficial);

/// <summary>
/// Period score row for a match.
/// </summary>
public record HockeyPeriodScoreDto(
    Guid Id,
    int PeriodNumber,
    string PeriodType,
    Guid HomeMatchTeamId,
    Guid AwayMatchTeamId,
    int HomeGoals,
    int AwayGoals,
    bool IsCompleted);
