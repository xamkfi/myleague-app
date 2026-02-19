using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Team
{
    /// <summary>
    /// Query for retrieving a floorball team by id
    /// </summary>
    /// <param name="Id"></param>
    public record GetFloorballTeamByIdQuery(Guid Id) : IRequest<Result<FloorballTeamDto>>;
}
