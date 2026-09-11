using Domain.Enums.Football;

namespace Application.Features.Football.Matches.DTOs;

/// <summary>
/// Per-period (half / extra-time / shootout) score snapshot.
/// </summary>
public record FootballPeriodScoreDto(int HomeScore, int AwayScore, bool IsCompleted);

/// <summary>
/// A player in a team's match squad.
/// </summary>
public record FootballLineupPlayerDto(
    Guid PlayerId,
    FootballPosition Position,
    bool IsOnField,
    bool IsSentOff);

/// <summary>
/// A goal scored in a football match.
/// </summary>
public record FootballGoalEventDto(
    Guid Id,
    Guid TeamId,
    Guid? ScoringPlayerId,
    Guid? AssistingPlayerId,
    int PeriodNumber,
    int TimeInSeconds,
    string PlayerName,
    string? AssisterName,
    FootballGoalType? GoalType,
    string? Description);

/// <summary>
/// A disciplinary card shown in a football match.
/// </summary>
public record FootballCardEventDto(
    Guid Id,
    Guid TeamId,
    Guid PlayerId,
    FootballCardType CardType,
    int PeriodNumber,
    int TimeInSeconds,
    string PlayerName,
    string? Description);

/// <summary>
/// A substitution in a football match.
/// </summary>
public record FootballSubstitutionEventDto(
    Guid Id,
    Guid TeamId,
    Guid PlayerOffId,
    Guid PlayerOnId,
    int PeriodNumber,
    int TimeInSeconds,
    string PlayerOffName,
    string PlayerOnName,
    string? Description);
