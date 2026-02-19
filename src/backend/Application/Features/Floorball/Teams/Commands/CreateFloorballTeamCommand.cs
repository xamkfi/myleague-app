using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Domain.Enums.Floorball;
using Domain.Enums.Common;
using MediatR;

namespace Application.Features.Floorball.Teams.Commands
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
    /// <param name="ShortName"></param>
    public record CreateFloorballTeamCommand(
        string Name,
        Guid? DivisionId,
        Guid ClubId,
        string HomeArena,
        string PrimaryJerseyColor,
        TeamCategory TeamCategory,
        string? SecondaryJerseyColor,
        string? ShortName) : IRequest<Result<FloorballTeamDto>>;
}
