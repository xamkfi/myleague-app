using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for deleting a penalty event from a non-event-sourced match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="PenaltyEventId"></param>
    public record DeletePenaltyCommand(
        Guid MatchId,
        Guid PenaltyEventId) : IRequest<Result<FloorballMatchDto>>;
}


