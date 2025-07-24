using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for changing the venue of an event-sourced floorball match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    /// <param name="NewVenue">The new venue</param>
    public record ChangeEventSourcedFloorballMatchVenueCommand(
        Guid MatchId,
        string NewVenue) : IRequest<Result<FloorballMatchDto>>;
} 