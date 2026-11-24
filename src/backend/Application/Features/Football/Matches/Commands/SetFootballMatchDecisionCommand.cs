using Application.Common;
using Application.Features.Football.Matches.DTOs;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

/// <summary>
/// Corrects the phase of play that decided a football match in progress.
/// </summary>
public record SetFootballMatchDecisionCommand(
    Guid MatchId,
    MatchDecision Decision) : IRequest<Result<FootballMatchDto>>;
