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

namespace Application.Features.Football.Teams.Commands
{
    /// <summary>
    /// Command for updating a player's information within a team
    /// </summary>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    /// <param name="Position"></param>
    /// <param name="JerseyNumber"></param>
    /// <param name="IsActive"></param>
    public record UpdateTeamPlayerCommand(
        Guid TeamId,
        Guid PlayerId,
        FootballPosition Position,
        int? JerseyNumber,
        bool IsActive) : IRequest<Result<FootballTeamPlayerDto>>;
} 
