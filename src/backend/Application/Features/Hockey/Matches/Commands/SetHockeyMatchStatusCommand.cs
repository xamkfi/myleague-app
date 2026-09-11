using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Hockey.Matches;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Sets the status of a hockey match.
/// </summary>
public record SetHockeyMatchStatusCommand(
    Guid MatchId,
    HockeyMatchStatus Status) : IRequest<Result<HockeyMatchDto>>;
