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
    /// Command for updating a football player
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Position"></param>
    /// <param name="IsActive"></param>
    public record UpdateFootballPlayerCommand(
        Guid Id,
        bool IsActive) : IRequest<Result<FootballPlayerDto>>;
}
