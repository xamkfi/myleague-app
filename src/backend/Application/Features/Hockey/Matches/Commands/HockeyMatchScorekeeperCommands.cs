using Application.Common;
using Application.Features.Hockey.Matches.DTOs;
using MediatR;

namespace Application.Features.Hockey.Matches.Commands;

/// <summary>
/// Adds a person as a scorekeeper (toimitsija) to a hockey match. Adding the same person twice is a no-op.
/// </summary>
/// <param name="MatchId">Target match.</param>
/// <param name="PersonId">Person acting as scorekeeper.</param>
public record AddHockeyMatchScorekeeperCommand(Guid MatchId, Guid PersonId) : IRequest<Result<HockeyMatchDto>>;

/// <summary>
/// Removes a scorekeeper (toimitsija) from a hockey match.
/// </summary>
/// <param name="MatchId">Target match.</param>
/// <param name="PersonId">Scorekeeper person to remove.</param>
public record RemoveHockeyMatchScorekeeperCommand(Guid MatchId, Guid PersonId) : IRequest<Result<HockeyMatchDto>>;
