using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.TeamManager
{
    /// <summary>
    /// Command for deleting a floorball team manager
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteFloorballTeamManagerCommand(Guid Id) : IRequest<Result<FloorballTeamManagerDto>>;
} 