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
    /// Query for retrieving all floorball teams
    /// </summary>
    public record GetAllFloorballTeamsQuery() : IRequest<Result<IEnumerable<FloorballTeamDto>>>;
}
