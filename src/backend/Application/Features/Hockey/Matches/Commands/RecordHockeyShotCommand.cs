using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a shot event on a hockey match. <paramref name="Count"/> above 1 records that many
/// identical saved shots at once (bulk torjunnat backfill).
/// </summary>
public record RecordHockeyShotCommand(
    Guid MatchId,
    Guid ShootingMatchTeamId,
    int PeriodNumber,
    int TimeInSeconds,
    HockeyShotResult ShotResult,
    bool CountsAsShotOnGoal,
    Guid? ShooterActivePlayerId = null,
    Guid? GoalieActivePlayerId = null,
    string? Description = null,
    int Count = 1) : IRequest<Result<HockeyMatchDto>>;
