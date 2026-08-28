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

namespace Application.Features.Football.TeamManagers.Commands
{
    /// <summary>
    /// Command for creating a new football team manager
    /// </summary>
    /// <param name="PersonId">The ID of the person who will be the team manager</param>
    /// <param name="TeamId">The ID of the team this manager will be responsible for</param>
    public record CreateFootballTeamManagerCommand(
        Guid PersonId,
        Guid TeamId) : IRequest<Result<FootballTeamManagerDto>>;
} 
