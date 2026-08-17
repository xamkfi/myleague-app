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

namespace Application.Features.Football.Players.Commands
{
    /// <summary>
    /// Command for creating a new football player
    /// </summary>
    /// <param name="PersonId"></param>
    public record CreateFootballPlayerCommand(
        Guid PersonId) : IRequest<Result<FootballPlayerDto>>;
}
