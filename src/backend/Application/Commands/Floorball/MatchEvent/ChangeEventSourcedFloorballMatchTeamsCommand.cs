using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for changing the teams of an event-sourced floorball match
    /// </summary>
    /// <param name="MatchId">The match ID</param>
    /// <param name="NewHomeTeamId">The new home team ID</param>
    /// <param name="NewAwayTeamId">The new away team ID</param>
    public record ChangeEventSourcedFloorballMatchTeamsCommand(
        Guid MatchId,
        Guid NewHomeTeamId,
        Guid NewAwayTeamId) : IRequest<Result<FloorballMatchDto>>;
} 