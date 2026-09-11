namespace Application.Features.Football.Matches.DTOs;

/// <summary>
/// Data Transfer Object for football match rules configuration.
/// </summary>
public record FootballMatchRulesDto(
    int NumberOfHalves,
    int HalfDurationMinutes,
    int PlayersOnField,
    bool RequireGoalkeeper,
    int MaxSubstitutions,
    bool RequireOfficialsToStart,
    bool AllowExtraTime,
    int ExtraTimeHalfCount,
    int ExtraTimeHalfDurationMinutes,
    bool AllowPenaltyShootout);
