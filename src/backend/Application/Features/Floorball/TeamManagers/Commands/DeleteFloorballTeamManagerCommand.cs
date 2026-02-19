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
    /// Command for deleting a floorball team manager
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteFloorballTeamManagerCommand(Guid Id) : IRequest<Result<FloorballTeamManagerDto>>;
} 
