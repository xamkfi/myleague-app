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
    /// Command for updating a player's statistics within a team
    /// </summary>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    /// <param name="GamesPlayed"></param>
    /// <param name="Goals"></param>
    /// <param name="Assists"></param>
    /// <param name="YellowCards"></param>
    /// <param name="RedCards"></param>
    public record UpdateTeamPlayerStatsCommand(
        Guid TeamId,
        Guid PlayerId,
        int GamesPlayed,
        int Goals,
        int Assists,
        int YellowCards,
        int RedCards) : IRequest<Result<FootballTeamPlayerDto>>;
} 
