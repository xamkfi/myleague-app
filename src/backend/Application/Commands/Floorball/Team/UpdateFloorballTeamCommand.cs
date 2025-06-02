using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Team
{
    /// <summary>
    /// Command for updating a floorball team
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="Division"></param>
    /// <param name="HomeArena"></param>
    /// <param name="PrimaryJerseyColor"></param>
    /// <param name="SecondaryJerseyColor"></param>
    public record UpdateFloorballTeamCommand(
        Guid Id,
        string Name,
        FloorballDivision Division,
        string HomeArena,
        string PrimaryJerseyColor,
        string? SecondaryJerseyColor) : IRequest<Result<FloorballTeamDto>>;
}
