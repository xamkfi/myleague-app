using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for canceling a non-event-sourced floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    public record CancelMatchCommand(Guid MatchId) : IRequest<Result<FloorballMatchDto>>;
}


