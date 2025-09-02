using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for adding an official to a non-event-sourced floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="RefereeId"></param>
    public record AddOfficialToMatchCommand(
        Guid MatchId,
        Guid RefereeId) : IRequest<Result<FloorballMatchDto>>;
}


