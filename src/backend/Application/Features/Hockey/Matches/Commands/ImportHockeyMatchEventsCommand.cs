using System.Collections.Generic;
using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Imports goals and penalties for an already-started hockey match in one unit of work.
/// Intended for historical backfill (JoomLeague importer), not live scorekeeping.
/// </summary>
public record ImportHockeyMatchEventsCommand(
    Guid MatchId,
    IReadOnlyList<ImportHockeyMatchEventItem> Events)
    : IRequest<Result<HockeyMatchEventsImportDto>>;

/// <summary>
/// One event in an <see cref="ImportHockeyMatchEventsCommand"/> batch.
/// <paramref name="EventType"/> is <c>Goal</c> or <c>Penalty</c>.
/// Player ids are dressed <c>HockeyMatchActivePlayer</c> ids, matching the live endpoints.
/// </summary>
public record ImportHockeyMatchEventItem(
    string EventType,
    Guid MatchTeamId,
    Guid? ActivePlayerId,
    Guid? PrimaryAssistActivePlayerId,
    Guid? SecondaryAssistActivePlayerId,
    Guid? GoalieActivePlayerId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyGoalStrength? GoalStrength,
    bool WasEmptyNet,
    string? Description,
    HockeyPenaltySeverity? Severity,
    HockeyPenaltyOffence? Offence,
    int? PenaltyMinutes,
    Guid? ServedByActivePlayerId,
    bool IsBenchPenalty);
