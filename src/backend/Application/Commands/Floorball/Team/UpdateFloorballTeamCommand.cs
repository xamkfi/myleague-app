using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using Domain.Enums.Floorball;
using Domain.Enums.Common;
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
    /// <param name="TeamCategory"></param>
    /// <param name="SecondaryJerseyColor"></param>
    /// <param name="LogoUrl"></param>
    /// <param name="ShortName"></param>
    public record UpdateFloorballTeamCommand(
        Guid Id,
        string Name,
        Guid? DivisionId,
        string HomeArena,
        string PrimaryJerseyColor,
        TeamCategory TeamCategory,
        string? SecondaryJerseyColor,
        string? LogoUrl,
        string? ShortName) : IRequest<Result<FloorballTeamDto>>;
}
