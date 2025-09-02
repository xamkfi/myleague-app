using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Match
{
    /// <summary>
    /// Command for deleting a goal event from a non-event-sourced match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="GoalEventId"></param>
    public record DeleteGoalCommand(
        Guid MatchId,
        Guid GoalEventId) : IRequest<Result<FloorballMatchDto>>;
}


