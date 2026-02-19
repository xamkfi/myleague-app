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
    /// Command for creating a new floorball team manager
    /// </summary>
    /// <param name="PersonId">The ID of the person who will be the team manager</param>
    /// <param name="TeamId">The ID of the team this manager will be responsible for</param>
    public record CreateFloorballTeamManagerCommand(
        Guid PersonId,
        Guid TeamId) : IRequest<Result<FloorballTeamManagerDto>>;
} 
