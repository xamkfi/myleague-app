using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for reactivating a cancelled floorball match back to Scheduled status
    /// </summary>
    /// <param name="MatchId"></param>
    public record ReactivateMatchCommand(Guid MatchId) : IRequest<Result<FloorballMatchDto>>;
}
