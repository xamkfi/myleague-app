using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for starting an event-sourced floorball match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    public record StartEventSourcedFloorballMatchCommand(
        Guid MatchId) : IRequest<Result<FloorballMatchDto>>;
} 