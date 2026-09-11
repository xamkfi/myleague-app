using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Sets whether a hockey match went to overtime.
/// </summary>
public record SetHockeyMatchWentToOvertimeCommand(
    Guid MatchId,
    bool WentToOvertime) : IRequest<Result<HockeyMatchDto>>;
