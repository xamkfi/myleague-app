using System;
using System.Collections.Generic;
using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

/// <summary>
/// Imports goals and cards for an already-started football match in one unit of work.
/// Intended for historical backfill, not live scorekeeping.
/// </summary>
public record ImportFootballMatchEventsCommand(
    Guid MatchId,
    IReadOnlyList<ImportFootballMatchEventItem> Events)
    : IRequest<Result<FootballMatchEventsImportDto>>;

/// <summary>
/// One event in an <see cref="ImportFootballMatchEventsCommand"/> batch.
/// <paramref name="EventType"/> is <c>Goal</c> or <c>Card</c>.
/// </summary>
public record ImportFootballMatchEventItem(
    string EventType,
    Guid TeamId,
    Guid? PlayerId,
    Guid? AssistingPlayerId,
    int PeriodNumber,
    int TimeInSeconds,
    FootballGoalType? GoalType,
    string? Description,
    FootballCardType? CardType);
