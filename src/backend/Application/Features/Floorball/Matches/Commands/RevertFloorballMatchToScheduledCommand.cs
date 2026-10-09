using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands;

/// <summary>
/// Puts a match that was started by mistake back to Scheduled. Allowed only while the match is
/// in progress with a 0-0 score and no recorded events.
/// </summary>
/// <param name="Id">Match identifier.</param>
public record RevertFloorballMatchToScheduledCommand(Guid Id) : IRequest<Result<FloorballMatchDto>>;
