using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Features.Floorball.Seasons.Queries
{
    /// <summary>
    /// Query for retrieving floorball seasons by division
    /// </summary>
    /// <param name="Division"></param>
    public record GetFloorballSeasonsByDivisionQuery(Guid DivisionId) : IRequest<Result<IEnumerable<FloorballSeasonDto>>>;
}
