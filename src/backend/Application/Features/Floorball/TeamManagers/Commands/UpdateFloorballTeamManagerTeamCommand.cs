using System;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using MediatR;

namespace Application.Features.Floorball.TeamManagers.Commands
{
    /// <summary>
    /// Command for updating the team assignment of a floorball team manager
    /// </summary>
    /// <param name="Id">The ID of the team manager to update</param>
    /// <param name="TeamId">The new team ID this manager will be responsible for</param>
    public record UpdateFloorballTeamManagerTeamCommand(
        Guid Id,
        Guid TeamId) : IRequest<Result<FloorballTeamManagerDto>>;
} 
