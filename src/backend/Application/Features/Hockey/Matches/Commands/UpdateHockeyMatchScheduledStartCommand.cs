using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Updates the scheduled start time of a hockey match.
/// </summary>
public record UpdateHockeyMatchScheduledStartCommand(
    Guid MatchId,
    DateTime ScheduledStartTime) : IRequest<Result<HockeyMatchDto>>;
