using Application.Common;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Command for permanently deleting a floorball match.
/// Used by the tournament-JSON import revert flow.
/// </summary>
/// <param name="MatchId">Match to delete</param>
public record DeleteFloorballMatchCommand(Guid MatchId) : IRequest<Result>;
