using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Records a goalie change event on a hockey match.
/// </summary>
public record RecordHockeyGoalieChangeCommand(
    Guid MatchId,
    Guid MatchTeamId,
    int PeriodNumber,
    int TimeInSeconds,
    Guid? OutgoingGoalieActivePlayerId = null,
    Guid? IncomingGoalieActivePlayerId = null,
    string? Reason = null,
    string? Description = null) : IRequest<Result<HockeyMatchDto>>;
