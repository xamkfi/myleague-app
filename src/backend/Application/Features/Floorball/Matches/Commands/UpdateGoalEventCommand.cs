using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for updating a goal event in a floorball match
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="AssisterId"></param>
    /// <param name="PeriodNumber"></param>
    /// <param name="TimeInSeconds"></param>
    /// <param name="WasInOvertime"></param>
    /// <param name="WasInShootout"></param>
    public record UpdateGoalEventCommand(
        Guid Id,
        Guid? AssisterId,
        int PeriodNumber,
        int TimeInSeconds,
        bool WasInOvertime,
        bool WasInShootout) : IRequest<Result<FloorballGoalEventDto>>;
} 