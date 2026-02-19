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
using MediatR;

namespace Application.Features.Floorball.Teams.Commands
{
    /// <summary>
    /// Command for adding a new player to a floorball team
    /// </summary>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    /// <param name="Position"></param>
    /// <param name="JerseyNumber"></param>
    public record AddPlayerToTeamCommand(
        Guid TeamId,
        Guid PlayerId,
        FloorballPosition Position,
        int? JerseyNumber) : IRequest<Result<FloorballTeamDto>>;
}
