using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Corrects the phase of play that decided a hockey match in progress.
/// </summary>
public record SetHockeyMatchDecisionCommand(Guid MatchId, MatchDecision Decision)
    : IRequest<Result<HockeyMatchDto>>;
