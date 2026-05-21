using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands
{
    /// <summary>
    /// Command for reopening a previously completed floorball match back to InProgress so the
    /// operator can correct mistakes (events, score) or continue recording. The handler is
    /// responsible for reverting the per-match aggregates applied at completion time.
    /// </summary>
    /// <param name="Id">Match identifier.</param>
    public record ReopenFloorballMatchCommand(Guid Id) : IRequest<Result<FloorballMatchDto>>;
}
