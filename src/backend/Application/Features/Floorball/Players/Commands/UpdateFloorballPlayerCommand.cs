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

namespace Application.Features.Floorball.Players.Commands
{
    /// <summary>
    /// Command for updating a floorball player
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Position"></param>
    /// <param name="IsActive"></param>
    public record UpdateFloorballPlayerCommand(
        Guid Id,
        bool IsActive) : IRequest<Result<FloorballPlayerDto>>;
}
