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
    /// <param name="Name"></param>
    /// <param name="Division"></param>
    /// <param name="StartDate"></param>
    /// <param name="EndDate"></param>
    public record CreateFloorballSeasonCommand(
        string Name,
        Guid DivisionId,
        DateTime StartDate,
        DateTime EndDate) : IRequest<Result<FloorballSeasonDto>>;
}
