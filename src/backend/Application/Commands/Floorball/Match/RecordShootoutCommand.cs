using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for recording shootout in a non-event-sourced floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    public record RecordShootoutCommand(Guid MatchId) : IRequest<Result<FloorballMatchDto>>;
}


