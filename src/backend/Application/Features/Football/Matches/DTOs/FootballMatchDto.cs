using Domain.Enums.Football;

namespace Application.Features.Football.Matches.DTOs;

/// <summary>
/// Data Transfer Object for FootballMatch entity.
/// </summary>
public record FootballMatchDto(
    Guid Id,
    Guid CompetitionId,
    string CompetitionName,
    Guid? HomeTeamId,
    string? HomeTeamName,
    Uri? HomeTeamLogo,
    Guid? AwayTeamId,
    string? AwayTeamName,
    Uri? AwayTeamLogo,
    DateTime ScheduledDateTime,
    string? Venue,
    FootballMatchStatus Status,
    int HomeScore,
    int AwayScore,
    bool WentToExtraTime,
    bool WentToPenaltyShootout,
    IReadOnlyDictionary<int, FootballPeriodScoreDto> PeriodScores,
    IReadOnlyCollection<Guid> Officials,
    IReadOnlyCollection<FootballGoalEventDto> GoalEvents,
    IReadOnlyCollection<FootballCardEventDto> CardEvents,
    IReadOnlyCollection<FootballSubstitutionEventDto> SubstitutionEvents,
    FootballMatchRulesDto MatchRules,
    IReadOnlyCollection<FootballLineupPlayerDto> HomeLineup,
    IReadOnlyCollection<FootballLineupPlayerDto> AwayLineup,
    Guid? TournamentGroupId = null,
    string? TournamentStage = null,
    FootballCompetitionType CompetitionType = FootballCompetitionType.Season,
    FootballPlayoffRound? PlayoffRound = null,
    int? PlayoffMatchOrder = null,
    Guid? NextMatchId = null,
    FootballPlayoffSlot? NextMatchSlot = null);
