using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Queries.Floorball.Season
{
    /// <summary>
    /// Query for retrieving floorball seasons by division
    /// </summary>
    /// <param name="Division"></param>
    public record GetFloorballSeasonsByDivisionQuery(FloorballDivision Division) : IRequest<Result<IEnumerable<FloorballSeasonDto>>>;
}
