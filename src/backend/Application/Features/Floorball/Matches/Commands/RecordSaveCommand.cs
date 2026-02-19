using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for recording a save in a non-event-sourced floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="TeamId"></param>
    /// <param name="GoalieId"></param>
    /// <param name="PeriodNumber"></param>
    /// <param name="TimeInSeconds"></param>
    /// <param name="WasInOvertime"></param>
    /// <param name="WasInShootout"></param>
    public record RecordSaveCommand(
        Guid MatchId,
        Guid TeamId,
        Guid GoalieId,
        int PeriodNumber,
        int TimeInSeconds,
        bool WasInOvertime,
        bool WasInShootout) : IRequest<Result<FloorballMatchDto>>;
}


