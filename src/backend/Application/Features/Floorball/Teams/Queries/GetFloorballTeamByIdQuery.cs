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
using MediatR;

namespace Application.Features.Floorball.Teams.Queries
{
    /// <summary>
    /// Query for retrieving a floorball team by id
    /// </summary>
    /// <param name="Id"></param>
    public record GetFloorballTeamByIdQuery(Guid Id) : IRequest<Result<FloorballTeamDto>>;
}
