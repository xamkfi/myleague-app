using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for recording shootout in an event-sourced floorball match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    public record RecordShootoutEventCommand(
        Guid MatchId) : IRequest<Result<FloorballMatchDto>>;
} 