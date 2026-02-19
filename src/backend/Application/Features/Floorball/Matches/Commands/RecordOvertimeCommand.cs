using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for recording overtime in a non-event-sourced floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    public record RecordOvertimeCommand(Guid MatchId) : IRequest<Result<FloorballMatchDto>>;
}


