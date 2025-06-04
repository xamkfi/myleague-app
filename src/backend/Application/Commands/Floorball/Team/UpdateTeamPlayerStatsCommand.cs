using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Floorball;
using MediatR;

namespace Application.Commands.Floorball.Team
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