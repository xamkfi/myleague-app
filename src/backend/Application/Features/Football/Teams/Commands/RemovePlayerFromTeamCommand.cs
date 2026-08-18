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

namespace Application.Features.Football.Teams.Commands
{
    /// <summary>
    /// Command for removing a player from a football team
    /// </summary>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    public record RemovePlayerFromTeamCommand(
        Guid TeamId,
        Guid PlayerId) : IRequest<Result<FootballTeamDto>>;
}
