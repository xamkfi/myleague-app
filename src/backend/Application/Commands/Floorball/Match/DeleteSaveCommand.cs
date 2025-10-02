using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for deleting a save event from a non-event-sourced match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="SaveEventId"></param>
    public record DeleteSaveCommand(
        Guid MatchId,
        Guid SaveEventId) : IRequest<Result<FloorballMatchDto>>;
}



