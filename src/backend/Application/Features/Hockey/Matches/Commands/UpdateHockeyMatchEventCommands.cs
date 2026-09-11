using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Corrects a goal event during live match operations.
/// </summary>
public record UpdateHockeyGoalCommand(
    Guid MatchId,
    Guid GoalEventId,
    Guid ScoringMatchTeamId,
    Guid ScorerActivePlayerId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyGoalStrength GoalStrength,
    Guid? PrimaryAssistActivePlayerId = null,
    Guid? SecondaryAssistActivePlayerId = null,
    Guid? GoalieActivePlayerId = null,
    bool WasEmptyNet = false,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;

/// <summary>
/// Corrects a penalty event during live match operations.
/// </summary>
public record UpdateHockeyPenaltyCommand(
    Guid MatchId,
    Guid PenaltyEventId,
    Guid PenaltyMatchTeamId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyPenaltySeverity Severity,
    HockeyPenaltyOffence Offence,
    int PenaltyMinutes,
    Guid? PenalizedActivePlayerId = null,
    Guid? ServedByActivePlayerId = null,
    bool IsBenchPenalty = false,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;

/// <summary>
/// Corrects a shot event during live match operations.
/// </summary>
public record UpdateHockeyShotCommand(
    Guid MatchId,
    Guid ShotEventId,
    Guid ShootingMatchTeamId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyShotResult ShotResult,
    bool CountsAsShotOnGoal,
    Guid? ShooterActivePlayerId = null,
    Guid? GoalieActivePlayerId = null,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;
