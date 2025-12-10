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
    /// Command for creating a new floorball team
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="DivisionId"></param>
    /// <param name="ClubId"></param>
    /// <param name="HomeArena"></param>
    /// <param name="PrimaryJerseyColor"></param>
    /// <param name="TeamCategory"></param>
    /// <param name="SecondaryJerseyColor"></param>
    public record CreateFloorballTeamCommand(
        string Name,
        Guid? DivisionId,
        Guid ClubId,
        string HomeArena,
        string PrimaryJerseyColor,
        TeamCategory TeamCategory,
        string? SecondaryJerseyColor) : IRequest<Result<FloorballTeamDto>>;
}
