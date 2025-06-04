using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Season
{
    /// <summary>
    /// Command for deleting a floorball season
    /// </summary>
    /// <param name="Id"></param>
    public record DeleteFloorballSeasonCommand(
        Guid Id) : IRequest<Result>;
}
