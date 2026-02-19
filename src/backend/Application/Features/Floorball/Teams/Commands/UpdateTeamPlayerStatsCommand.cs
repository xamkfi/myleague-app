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

namespace Application.Features.Floorball.Teams.Commands
{
    /// <summary>
    /// Command for updating a player's statistics within a team
    /// </summary>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    /// <param name="GamesPlayed"></param>
    /// <param name="Goals"></param>
    /// <param name="Assists"></param>
    /// <param name="PenaltyMinutes"></param>
    public record UpdateTeamPlayerStatsCommand(
        Guid TeamId,
        Guid PlayerId,
        int GamesPlayed,
        int Goals,
        int Assists,
        int PenaltyMinutes) : IRequest<Result<FloorballTeamPlayerDto>>;
} 
