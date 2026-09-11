using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Marks a hockey match as started (in progress).
/// </summary>
public record MarkHockeyMatchStartedCommand(
    Guid MatchId,
    DateTime? ActualStartTime = null) : IRequest<Result<HockeyMatchDto>>;
