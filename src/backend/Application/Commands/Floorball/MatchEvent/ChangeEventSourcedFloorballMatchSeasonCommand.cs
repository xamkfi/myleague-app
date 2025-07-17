using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for changing the season of an event-sourced floorball match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    /// <param name="NewSeasonId">The new season ID</param>
    public record ChangeEventSourcedFloorballMatchSeasonCommand(
        Guid MatchId,
        Guid NewSeasonId) : IRequest<Result<FloorballMatchDto>>;
} 