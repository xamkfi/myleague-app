using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Team
{
    /// <summary>
    /// Command for deletin a floorball team
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteFloorballTeamCommand(Guid Id) : IRequest<Result>;
}
