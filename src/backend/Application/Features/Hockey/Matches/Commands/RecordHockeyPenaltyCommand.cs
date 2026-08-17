using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a penalty event on a hockey match.
/// </summary>
public record RecordHockeyPenaltyCommand(
    Guid MatchId,
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
