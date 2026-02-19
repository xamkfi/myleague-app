using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Command for updating a floorball team manager
    /// </summary>
    /// <param name="Id">The ID of the team manager to update</param>
    /// <param name="IsActive">Whether the team manager is active</param>
    public record UpdateFloorballTeamManagerCommand(
        Guid Id,
        bool IsActive) : IRequest<Result<FloorballTeamManagerDto>>;
} 
