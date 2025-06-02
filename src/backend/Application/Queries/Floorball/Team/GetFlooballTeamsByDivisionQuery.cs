using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Team
{
    /// <summary>
    /// Query for retrieving all floorball teams in a division
    /// </summary>
    /// <param name="Division"></param>
    public record GetFlooballTeamsByDivisionQuery(FloorballDivision Division) : IRequest<Result<IEnumerable<FloorballTeamDto>>>;
}
