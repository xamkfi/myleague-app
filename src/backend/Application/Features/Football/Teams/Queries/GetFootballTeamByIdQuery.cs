using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using MediatR;

namespace Application.Features.Football.Teams.Queries
{
    /// <summary>
    /// Query for retrieving a football team by id
    /// </summary>
    /// <param name="Id"></param>
    public record GetFootballTeamByIdQuery(Guid Id) : IRequest<Result<FootballTeamDto>>;
}
