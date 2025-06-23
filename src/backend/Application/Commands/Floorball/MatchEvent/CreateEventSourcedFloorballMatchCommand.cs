using System;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.MatchEvent
{
    /// <summary>
    /// Command for creating a new event-sourced floorball match
    /// </summary>
    /// <param name="Id">The match ID</param>
    /// <param name="SeasonId">The season ID</param>
    /// <param name="HomeTeamId">The home team ID</param>
    /// <param name="AwayTeamId">The away team ID</param>
    /// <param name="ScheduledDateTime">The scheduled date and time</param>
    /// <param name="Venue">The venue</param>
    public record CreateEventSourcedFloorballMatchCommand(
        Guid Id,
        Guid SeasonId,
        Guid HomeTeamId,
        Guid AwayTeamId,
        DateTime ScheduledDateTime,
        string Venue) : IRequest<Result<FloorballMatchDto>>;
} 