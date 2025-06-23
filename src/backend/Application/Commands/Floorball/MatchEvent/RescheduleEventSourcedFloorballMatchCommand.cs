using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for rescheduling an event-sourced floorball match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    /// <param name="NewDateTime">The new date and time</param>
    /// <param name="NewVenue">The new venue (optional)</param>
    public record RescheduleEventSourcedFloorballMatchCommand(
        Guid MatchId,
        DateTime NewDateTime,
        string? NewVenue = null) : IRequest<Result<FloorballMatchDto>>;
} 