using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Season
{
    /// <summary>
    /// Command for creating a floorball season
    /// </summary>
    /// <param name="Name">The name of the season</param>
    /// <param name="DivisionIds">The list of division IDs to associate with this season</param>
    /// <param name="StartDate">The start date of the season</param>
    /// <param name="EndDate">The end date of the season</param>
    public record CreateFloorballSeasonCommand(
        string Name,
        IEnumerable<Guid> DivisionIds,
        DateTime StartDate,
        DateTime EndDate) : IRequest<Result<FloorballSeasonDto>>;
}
