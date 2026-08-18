using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a shootout attempt event on a hockey match.
/// </summary>
public record RecordHockeyShootoutAttemptCommand(
    Guid MatchId,
    Guid MatchTeamId,
    Guid ShooterActivePlayerId,
    Guid GoalieActivePlayerId,
    int PeriodNumber,
    int TimeInSeconds,
    int ShotOrder,
    HockeyShootoutAttemptResult Result,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;
