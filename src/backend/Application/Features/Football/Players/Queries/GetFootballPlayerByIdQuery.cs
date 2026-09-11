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

namespace Application.Features.Football.Players.Queries
{
    /// <summary>
    /// Query for retrieving a football player by Id
    /// </summary>
    /// <param name="Id"></param>
    public record GetFootballPlayerByIdQuery(Guid Id) : IRequest<Result<FootballPlayerDto>>;
}
