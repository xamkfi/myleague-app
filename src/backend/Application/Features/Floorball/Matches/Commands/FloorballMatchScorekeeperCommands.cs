using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Adds a person as a scorekeeper (toimitsija) to a floorball match. Adding the same person twice is a no-op.
/// </summary>
/// <param name="MatchId">Target match.</param>
/// <param name="PersonId">Person acting as scorekeeper.</param>
public record AddFloorballScorekeeperToMatchCommand(Guid MatchId, Guid PersonId) : IRequest<Result<FloorballMatchDto>>;

/// <summary>
/// Removes a scorekeeper (toimitsija) from a floorball match.
/// </summary>
/// <param name="MatchId">Target match.</param>
/// <param name="PersonId">Scorekeeper person to remove.</param>
public record RemoveFloorballScorekeeperFromMatchCommand(Guid MatchId, Guid PersonId) : IRequest<Result<FloorballMatchDto>>;
