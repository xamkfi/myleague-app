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
using Domain.Enums.Football;
using MediatR;

namespace Application.Features.Football.Teams.Queries
{
    /// <summary>
    /// Query for retrieving all football teams in a division
    /// </summary>
    /// <param name="Division"></param>
    public record GetFootballTeamsByDivisionQuery(Guid DivisionId) : IRequest<Result<IEnumerable<FootballTeamDto>>>;
}
