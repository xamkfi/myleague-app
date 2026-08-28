using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Marks a hockey match as finished.
/// </summary>
public record MarkHockeyMatchFinishedCommand(
    Guid MatchId,
    DateTime? ActualEndTime = null,
    HockeyMatchResultType? ResultType = null) : IRequest<Result<HockeyMatchDto>>;
