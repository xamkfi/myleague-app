using Application.Common;
using Application.Features.Football.Matches.DTOs;
using MediatR;

namespace Application.Features.Football.Matches.Commands;

/// <summary>
/// Adds a person as a scorekeeper (toimitsija) to a football match. Adding the same person twice is a no-op.
/// </summary>
/// <param name="MatchId">Target match.</param>
/// <param name="PersonId">Person acting as scorekeeper.</param>
public record AddFootballScorekeeperToMatchCommand(Guid MatchId, Guid PersonId) : IRequest<Result<FootballMatchDto>>;

/// <summary>
/// Removes a scorekeeper (toimitsija) from a football match.
/// </summary>
/// <param name="MatchId">Target match.</param>
/// <param name="PersonId">Scorekeeper person to remove.</param>
public record RemoveFootballScorekeeperFromMatchCommand(Guid MatchId, Guid PersonId) : IRequest<Result<FootballMatchDto>>;
