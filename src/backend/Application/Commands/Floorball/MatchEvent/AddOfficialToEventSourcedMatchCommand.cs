using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for adding an official to an event-sourced floorball match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    /// <param name="RefereeId">The referee ID</param>
    public record AddOfficialToEventSourcedMatchCommand(
        Guid MatchId,
        Guid RefereeId) : IRequest<Result<FloorballMatchDto>>;
} 