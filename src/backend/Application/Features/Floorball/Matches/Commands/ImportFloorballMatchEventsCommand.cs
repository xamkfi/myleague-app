using System;
using System.Collections.Generic;
using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Imports goals and penalties for an already-started floorball match in one unit of work.
/// Intended for historical backfill (JoomLeague importer), not live scorekeeping.
/// </summary>
public record ImportFloorballMatchEventsCommand(
    Guid MatchId,
    IReadOnlyList<ImportFloorballMatchEventItem> Events)
    : IRequest<Result<FloorballMatchEventsImportDto>>;

/// <summary>
/// One event in an <see cref="ImportFloorballMatchEventsCommand"/> batch.
/// <paramref name="EventType"/> is <c>Goal</c> or <c>Penalty</c>.
/// </summary>
public record ImportFloorballMatchEventItem(
    string EventType,
    Guid TeamId,
    Guid? PlayerId,
    Guid? AssistingPlayerId,
    Guid? SecondaryAssistingPlayerId,
    int PeriodNumber,
    int TimeInSeconds,
    int? GoalType,
    string? Description,
    int? PenaltyMinutes,
    string? PenaltyType);
