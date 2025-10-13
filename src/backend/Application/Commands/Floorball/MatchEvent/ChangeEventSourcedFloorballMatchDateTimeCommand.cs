using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for changing the date/time of an event-sourced floorball match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    /// <param name="NewDateTime">The new date and time</param>
    public record ChangeEventSourcedFloorballMatchDateTimeCommand(
        Guid MatchId,
        DateTime NewDateTime) : IRequest<Result<FloorballMatchDto>>;
} 