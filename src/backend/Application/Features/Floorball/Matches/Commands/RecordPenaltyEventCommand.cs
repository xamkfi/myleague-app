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
using Domain.Enums.Floorball;
using MediatR;

namespace Application.Features.Floorball.Matches.Commands
{
    /// <summary>
    /// Command for recording a penalty event in a floorball match
    /// </summary>
    /// <param name="MatchId"></param>
    /// <param name="TeamId"></param>
    /// <param name="PlayerId"></param>
    /// <param name="PenaltyType"></param>
    /// <param name="Minutes"></param>
    /// <param name="PeriodNumber"></param>
    /// <param name="TimeInSeconds"></param>
    /// <param name="Description"></param>
    public record RecordPenaltyEventCommand(
        Guid MatchId,
        Guid TeamId,
        Guid? PlayerId,
        FloorballPenaltyType PenaltyType,
        int Minutes,
        int PeriodNumber,
        int TimeInSeconds,
        string Description) : IRequest<Result<FloorballPenaltyEventDto>>;
} 
